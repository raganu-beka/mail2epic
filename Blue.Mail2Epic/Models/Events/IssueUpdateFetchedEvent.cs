using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Models.Dtos.AzureAi;
using Blue.Mail2Epic.Models.Dtos.Email;

namespace Blue.Mail2Epic.Models.Events;

public class IssueUpdateFetchedEvent
{
    public required List<int> RecipientUserAccountIds { get; set; }
    public required EmailDataDto EmailData { get; set; }
    public required string IssueKey { get; set; }
    public required AzureAiIssueUpdateResponse IssueUpdate { get; set; }
    public required RequiredAction ActionTaken { get; set; }
}