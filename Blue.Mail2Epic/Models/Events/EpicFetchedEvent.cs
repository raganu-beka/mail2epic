using Blue.Mail2Epic.Infrastructure.Models.Responses;

namespace Blue.Mail2Epic.Models.Events;

public class EpicFetchedEvent
{
    public required JiraIssueResponse Epic { get; set; }
}