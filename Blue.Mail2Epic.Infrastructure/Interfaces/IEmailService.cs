using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IEmailService
{
    Task<List<EmailDataDto>> ReadInboxAsync(
        string emailAddress,
        int userAccountId,
        string accessToken,
        DateTimeOffset? fromTime,
        CancellationToken ct);

    List<(EmailDataDto Email, List<int> RecipientUserAccountIds)> FilterEmails(List<EmailDataDto> emails);
}
