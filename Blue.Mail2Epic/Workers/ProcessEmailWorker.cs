using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Events;
using Blue.Mail2Epic.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Blue.Mail2Epic.Workers;

public class ProcessEmailWorker(
    ILogger<ProcessEmailWorker> logger,
    AppDbContext dbContext,
    EmailTriageService emailTriageService,
    IBus bus
) : IConsumer<EmailFetchedEvent>
{
    public async Task Consume(ConsumeContext<EmailFetchedEvent> context)
    {
        var email = context.Message.Email;

        if (await dbContext.EmailMappingExistsAsync(email.MessageId, context.CancellationToken))
        {
            logger.LogInformation("E-mail [{Subject}] from [{From}] already processed. Skipping further processing.",
                email.Subject, email.From);
            return;
        }

        logger.LogInformation("Processing email [{Subject}] from [{From}]", email.Subject, email.From);

        try
        {
            var requiredAction = await emailTriageService.DetermineRequiredAction(email, context.CancellationToken);
            switch (requiredAction.RequiredAction)
            {
                case RequiredAction.AppendExistingIssue:
                    await AppendToExistingIssue(context.Message, requiredAction.IssueKey!, context.CancellationToken);
                    break;

                case RequiredAction.CreateNewIssue:
                    await CreateNewIssue(context.Message, context.CancellationToken);
                    break;

                case RequiredAction.NoAction:
                    logger.LogInformation("No action required for email {MessageId}", email.MessageId);
                    await SaveNoActionMapping(context.Message, context.CancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        $"Unexpected required action: {requiredAction.RequiredAction}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process email [{Subject}] from [{From}]", email.Subject, email.From);
            throw;
        }
    }

    private async Task AppendToExistingIssue(EmailFetchedEvent emailFetchedEvent, string issueKey, CancellationToken ct)
    {
        var issueUpdate = await emailTriageService.GetIssueUpdate(emailFetchedEvent.Email, issueKey, ct);
        await bus.Publish(new IssueUpdateFetchedEvent
        {
            RecipientUserAccountIds = emailFetchedEvent.RecipientUserAccountIds,
            EmailData = emailFetchedEvent.Email,
            IssueKey = issueKey,
            IssueUpdate = issueUpdate,
            ActionTaken = RequiredAction.AppendExistingIssue
        }, ct);
    }

    private async Task CreateNewIssue(EmailFetchedEvent emailFetchedEvent, CancellationToken ct)
    {
        var emailAnalysisResult = await emailTriageService.GetEmailAnalysis(emailFetchedEvent.Email, ct);
        await bus.Publish(new EmailAnalysisResultFetchedEvent
        {
            RecipientUserAccountIds = emailFetchedEvent.RecipientUserAccountIds,
            EmailData = emailFetchedEvent.Email,
            AnalysisResult = emailAnalysisResult,
            ActionTaken = RequiredAction.CreateNewIssue
        }, ct);
    }

    private async Task SaveNoActionMapping(EmailFetchedEvent emailFetchedEvent, CancellationToken ct)
    {
        dbContext.EmailMappings.Add(new EmailMapping
        {
            MessageId = emailFetchedEvent.Email.MessageId,
            MessageSenderAddress = emailFetchedEvent.Email.From,
            MessageSubject = emailFetchedEvent.Email.Subject,
            ThreadRootMessageId = emailFetchedEvent.Email.ThreadRootMessageId,
            JiraIssueKey = null,
            ActionTaken = RequiredAction.NoAction,
            Reasoning = "E-mail classified as not requiring action",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(ct);
    }
}