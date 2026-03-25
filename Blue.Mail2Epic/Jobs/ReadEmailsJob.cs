using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Services;
using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Models.Dtos.Email;
using Blue.Mail2Epic.Models.Events;
using Blue.Mail2Epic.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Blue.Mail2Epic.Jobs;

[DisallowConcurrentExecution]
public class ReadEmailsJob(
    ILogger<ReadEmailsJob> logger,
    AppDbContext dbContext,
    IOptions<EmailOptions> emailOptions,
    EmailService emailService,
    GoogleTokenService googleTokenService,
    IBus bus) : BaseJob
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var jobStartTime = DateTimeOffset.Now;
        logger.LogInformation("ReadEmails job started at: {Time}", jobStartTime);

        var lastExecutionTime = await GetLastExecutionTimeAsync(dbContext);

        var inboxes = await dbContext.GoogleMailboxAccounts
            .Where(x => x.UserAccount.IsActive)
            .ToListAsync();

        List<EmailDataDto> emails = [];
        foreach (var inbox in inboxes)
        {
            var token = await googleTokenService.GetValidAccessTokenAsync(inbox.Id, context.CancellationToken);
            var userEmails = await emailService.ReadInboxAsync(inbox.EmailAddress, inbox.UserAccountId, token,
                lastExecutionTime, context.CancellationToken);
            emails.AddRange(userEmails);

            if (inbox != inboxes.Last()) await Task.Delay(emailOptions.Value.DelayBetweenInboxesMs);
        }

        var filteredEmails = emailService.FilterEmails(emails);
        foreach (var (email, recipientUserAccountIds) in filteredEmails)
            await bus.Publish(new EmailFetchedEvent
            {
                Email = email,
                RecipientUserAccountIds = recipientUserAccountIds
            });

        if (emails.Count > 0)
            await UpdateJobLastRunTime(jobStartTime, dbContext);
    }
}