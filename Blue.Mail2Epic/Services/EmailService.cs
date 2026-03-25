using Blue.Mail2Epic.Models.Configuration;
using Blue.Mail2Epic.Models.Dtos.Email;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MessageSummaryItems = MailKit.MessageSummaryItems;

namespace Blue.Mail2Epic.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
{
    private readonly EmailOptions _options = options.Value;

    public async Task<List<EmailDataDto>> ReadInboxAsync(
        string emailAddress,
        int userAccountId,
        string accessToken,
        DateTimeOffset? fromTime,
        CancellationToken ct)
    {
        using var client = new ImapClient();

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.SslOnConnect,
            ct);

        var oauth2 = new SaslMechanismOAuth2(emailAddress, accessToken);
        await client.AuthenticateAsync(oauth2, ct);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

        var readFromTime = fromTime ?? DateTimeOffset.UtcNow.AddHours(-1);
        var (fetchCount, intervalHours) = GetFetchCount(readFromTime);

        logger.LogInformation(
            "Fetching last {FetchCount} messages from [{EmailAddress}] (interval: {IntervalHours:F1}h, cap: {Cap})",
            fetchCount, emailAddress, intervalHours, _options.MaxEmailsAbsoluteCap);

        if (fetchCount <= 0) return [];

        var messages = await GetMessageFromTime(inbox, readFromTime, fetchCount, ct);
        var emails = new List<EmailDataDto>();
        foreach (var message in messages)
        {
            var email = await inbox.GetMessageAsync(message.Index, ct);
            logger.LogInformation("Email from: {From}, Subject: {Subject}, Date: {Date}",
                email.From.ToString(), email.Subject, email.Date.ToString("u"));

            if (email.MessageId is not null)
                emails.Add(CreateEmailDataDto(email, userAccountId));
        }

        await client.DisconnectAsync(true, ct);
        return emails;
    }

    public List<(EmailDataDto Email, List<int> RecipientUserAccountIds)> FilterEmails(List<EmailDataDto> emails)
    {
        return emails
            .Where(e => !_options.IgnoredEmailAddresses.Any(ignored => e.From.Contains(ignored)))
            .GroupBy(e => e.MessageId)
            .Select(g => (
                Email: g.First(),
                RecipientUserAccountIds: g.Select(e => e.UserAccountId).Distinct().ToList()
            ))
            .ToList();
    }

    private EmailDataDto CreateEmailDataDto(MimeMessage email, int userAccountId)
    {
        return new EmailDataDto
        {
            MessageId = email.MessageId!,
            UserAccountId = userAccountId,
            InReplyTo = email.InReplyTo,
            References = email.References?.ToList() ?? [],
            From = email.From?.FirstOrDefault()?.ToString() ?? string.Empty,
            Subject = email.Subject ?? string.Empty,
            Body = email.TextBody ?? email.HtmlBody ?? string.Empty,
            Date = email.Date.UtcDateTime
        };
    }

    private (int fetchCount, double intervalHours) GetFetchCount(DateTimeOffset? fromTime)
    {
        if (!fromTime.HasValue) return (_options.MaxEmailsAbsoluteCap, 1.0);

        var intervalHours = (DateTimeOffset.UtcNow - fromTime.Value).TotalHours;
        var fetchCount = (int)(intervalHours * _options.MaxEmailsPerHour);
        return (Math.Min(fetchCount, _options.MaxEmailsAbsoluteCap), intervalHours);
    }

    private async Task<IEnumerable<IMessageSummary>> GetMessageFromTime(
        IMailFolder inbox,
        DateTimeOffset fromTime,
        int fetchCount,
        CancellationToken ct)
    {
        var startIndex = Math.Max(0, inbox.Count - fetchCount);
        var recentMessages = await inbox.FetchAsync(startIndex, -1,
            new FetchRequest(MessageSummaryItems.Envelope | MessageSummaryItems.InternalDate),
            ct);

        return recentMessages
            .Where(m => m.InternalDate.HasValue && m.InternalDate.Value >= fromTime)
            .ToList();
    }
}