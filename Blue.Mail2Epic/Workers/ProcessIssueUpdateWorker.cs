using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Blue.Mail2Epic.Workers;

public class ProcessIssueUpdateWorker(
    ILogger<ProcessIssueUpdateWorker> logger,
    AppDbContext dbContext,
    IJiraService jiraService) : IConsumer<IssueUpdateFetchedEvent>
{
    public async Task Consume(ConsumeContext<IssueUpdateFetchedEvent> context)
    {
        var email = context.Message.EmailData;

        if (await dbContext.EmailMappingExistsAsync(email.MessageId, context.CancellationToken))
        {
            logger.LogInformation("E-mail [{Subject}] from [{From}] already processed. Skipping issue update creation.",
                email.Subject, email.From);
            return;
        }

        var issueUpdate = context.Message.IssueUpdate;

        if (!issueUpdate.RequiresAction)
        {
            logger.LogInformation(
                "E-mail [{Subject}] from [{From}] classified as not requiring action. Skipping issue update creation.",
                email.Subject, email.From);
            await SaveNoActionMapping(context.Message, context.CancellationToken);
            return;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(issueUpdate.Comment))
                await jiraService.PostIssueComment(issueUpdate.Comment, context.Message.IssueKey,
                    context.CancellationToken);

            logger.LogInformation("Updated issue [{IssueKey}] from e-mail [{Subject}] from [{From}]",
                context.Message.IssueKey, email.Subject, email.From);

            var emailMapping = new EmailMapping
            {
                MessageId = email.MessageId,
                MessageSenderAddress = email.From,
                MessageSubject = email.Subject,
                ThreadRootMessageId = email.ThreadRootMessageId,
                JiraIssueKey = context.Message.IssueKey,
                ActionTaken = context.Message.ActionTaken,
                Reasoning = issueUpdate.Reasoning,
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

    private async Task SaveNoActionMapping(IssueUpdateFetchedEvent issueUpdateFetched, CancellationToken ct)
    {
        dbContext.EmailMappings.Add(new EmailMapping
        {
            MessageId = issueUpdateFetched.EmailData.MessageId,
            MessageSenderAddress = issueUpdateFetched.EmailData.From,
            MessageSubject = issueUpdateFetched.EmailData.Subject,
            ThreadRootMessageId = issueUpdateFetched.EmailData.ThreadRootMessageId,
            JiraIssueKey = issueUpdateFetched.IssueKey,
            ActionTaken = RequiredAction.NoAction,
            Reasoning = issueUpdateFetched.IssueUpdate.Reasoning,
            CreatedAt = DateTimeOffset.UtcNow,
            Recipients = issueUpdateFetched.RecipientUserAccountIds
                .Select(id => new EmailMappingRecipient { UserAccountId = id })
                .ToList()
        });
        await dbContext.SaveChangesAsync(ct);
    }
}
