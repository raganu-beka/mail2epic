using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Requests;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Blue.Mail2Epic.Infrastructure.Services;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Models.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blue.Mail2Epic.Workers;

public class ProcessIssueWorker(
    ILogger<ProcessIssueWorker> logger,
    IOptions<AdditionalOptions> options,
    AppDbContext dbContext,
    JiraService jiraService) : IConsumer<EmailAnalysisResultFetchedEvent>
{
    private readonly AdditionalOptions _options = options.Value;

    public async Task Consume(ConsumeContext<EmailAnalysisResultFetchedEvent> context)
    {
        var email = context.Message.EmailData;

        if (await dbContext.EmailMappingExistsAsync(email.MessageId, context.CancellationToken))
        {
            logger.LogInformation("E-mail [{Subject}] from [{From}] already processed. Skipping issue creation.",
                email.Subject, email.From);
            return;
        }

        var analysisResult = context.Message.AnalysisResult;

        if (!analysisResult.RequiresAction)
        {
            logger.LogInformation(
                "E-mail [{Subject}] from [{From}] classified as not requiring action. Skipping issue creation.",
                email.Subject, email.From);
            await SaveNoActionMapping(context.Message, context.CancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(analysisResult.EpicKey))
        {
            logger.LogInformation(
                "E-mail [{Subject}] from [{From}] did not get assigned epic. Skipping issue creation.", email.Subject,
                email.From);
            await SaveNoActionMapping(context.Message, context.CancellationToken);
            return;
        }

        if (analysisResult.EpicConfidence is null or < 50)
        {
            logger.LogInformation(
                "E-mail [{Subject}] from [{From}] classified with low confidence for epic relevance ({EpicConfidence}%). Skipping issue creation.",
                email.Subject, email.From, analysisResult.EpicConfidence ?? 0);
            await SaveNoActionMapping(context.Message, context.CancellationToken);
            return;
        }

        try
        {
            var epicIssues = await jiraService.GetIssuesByEpic(
                analysisResult.EpicKey,
                _options.ReporterLookupIssueCount ?? 5,
                context.CancellationToken);

            var issueRequest = new JiraIssuePostRequest
            {
                Summary = analysisResult.SuggestedTitle ?? string.Empty,
                Description = analysisResult.SuggestedDescription ?? string.Empty,
                EpicKey = analysisResult.EpicKey ?? string.Empty,
                Assignee = GetMostFrequentReporter(epicIssues)
            };
            var issueKey = await jiraService.PostIssue(issueRequest, false, context.CancellationToken);
            if (issueKey is null)
            {
                logger.LogError("Failed to create new issue from e-mail [{Subject}] from [{From}]", email.Subject,
                    email.From);
                return;
            }

            if (analysisResult.SuggestedDescription is not null)
                await jiraService.PostIssueComment(analysisResult.SuggestedDescription, issueKey,
                    context.CancellationToken);

            logger.LogInformation("Created new issue [{IssueKey}] from e-mail [{Subject}] from [{From}]", issueKey,
                email.Subject, email.From);

            var emailMapping = new EmailMapping
            {
                MessageId = email.MessageId,
                MessageSenderAddress = email.From,
                MessageSubject = email.Subject,
                ThreadRootMessageId = email.ThreadRootMessageId,
                JiraIssueKey = issueKey,
                ActionTaken = context.Message.ActionTaken,
                SuggestedEpicKey = analysisResult.EpicKey,
                EpicConfidence = analysisResult.EpicConfidence,
                Reasoning = analysisResult.Reasoning,
                CreatedAt = DateTimeOffset.UtcNow,
                Recipients = context.Message.RecipientUserAccountIds
                    .Select(id => new EmailMappingRecipient { UserAccountId = id })
                    .ToList()
            };
            dbContext.EmailMappings.Add(emailMapping);

            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process new issue from e-mail [{Subject}] from [{From}]", email.Subject,
                email.From);
            throw;
        }
    }

    private static string? GetMostFrequentReporter(List<JiraIssueResponse> issues)
    {
        return issues
            .Select(i => i.Fields?.Reporter?.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .GroupBy(name => name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }

    private async Task SaveNoActionMapping(EmailAnalysisResultFetchedEvent analysisResultFetched, CancellationToken ct)
    {
        dbContext.EmailMappings.Add(new EmailMapping
        {
            MessageId = analysisResultFetched.EmailData.MessageId,
            MessageSenderAddress = analysisResultFetched.EmailData.From,
            MessageSubject = analysisResultFetched.EmailData.Subject,
            ThreadRootMessageId = analysisResultFetched.EmailData.ThreadRootMessageId,
            JiraIssueKey = null,
            ActionTaken = RequiredAction.NoAction,
            SuggestedEpicKey = analysisResultFetched.AnalysisResult.EpicKey,
            EpicConfidence = analysisResultFetched.AnalysisResult.EpicConfidence,
            Reasoning = analysisResultFetched.AnalysisResult.Reasoning,
            CreatedAt = DateTimeOffset.UtcNow,
            Recipients = analysisResultFetched.RecipientUserAccountIds
                .Select(id => new EmailMappingRecipient { UserAccountId = id })
                .ToList()
        });
        await dbContext.SaveChangesAsync(ct);
    }
}