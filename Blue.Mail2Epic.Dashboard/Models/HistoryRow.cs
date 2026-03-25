using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Responses;

namespace Blue.Mail2Epic.Dashboard.Models;

public class HistoryRow
{
    public required string MessageSender { get; set; }
    public required string MessageSubject { get; set; }
    public string? JiraIssueKey { get; set; }
    public required RequiredAction ActionTaken { get; set; }
    public required DateTimeOffset ProcessedAt { get; set; }
    
    public string? JiraIssueLink { get; set; }
    public string GmailEmailLink { get; set; } = "";
    
    public AiDecisionDetails AiDecisionDetails { get; set; } = new AiDecisionDetails();
    public IssueEpic? IssueEpic { get; set; }

    public static HistoryRow From(EmailMapping emailMapping)
    {
        return new HistoryRow
        {
            MessageSender = emailMapping.MessageSenderAddress,
            MessageSubject = emailMapping.MessageSubject,
            JiraIssueKey = emailMapping.JiraIssueKey,
            ActionTaken = emailMapping.ActionTaken,
            ProcessedAt = emailMapping.CreatedAt,
            AiDecisionDetails = new AiDecisionDetails
            {
                SuggestedEpicKey = emailMapping.SuggestedEpicKey,
                EpicConfidence = emailMapping.EpicConfidence,
                ActionReasoning = emailMapping.Reasoning
            }
        };
    }
}

public class AiDecisionDetails
{
    public string? SuggestedEpicKey { get; set; }
    public int? EpicConfidence { get; set; }
    public string? ActionReasoning { get; set; }
}

public class IssueEpic
{
    public string? Key { get; set; }
    public string? Summary { get; set; }
    public string? Link { get; set; }

    public static IssueEpic From(JiraIssueResponse epicIssue, string link)
    {
        return new IssueEpic
        {
            Key = epicIssue.Key,
            Summary = epicIssue.Fields?.Summary,
            Link = link
        };
    }
}