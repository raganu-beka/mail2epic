using Blue.Mail2Epic.Infrastructure.Models;

namespace Blue.Mail2Epic.Dashboard.Models;

public class AccountData
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public bool GoogleMailboxAccountConnected { get; set; }
    public DateTimeOffset? LastEmailProcessingAt { get; set; }

    public static AccountData From(UserAccount userAccount, DateTimeOffset? lastEmailProcessingAt = null)
    {
        return new AccountData
        {
            Id = userAccount.Id,
            Email = userAccount.Email,
            DisplayName = userAccount.GoogleMailboxAccount?.DisplayName,
            IsActive = userAccount.IsActive,
            IsAdmin = userAccount.IsAdmin,
            LastUpdatedAt = userAccount.ModifiedAt,
            LastLoginAt = userAccount.LastLoginAt,
            GoogleMailboxAccountConnected = userAccount.GoogleMailboxAccount is not null,
            LastEmailProcessingAt = lastEmailProcessingAt
        };
    }
}