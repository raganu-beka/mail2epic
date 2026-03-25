namespace Blue.Mail2Epic.Infrastructure.Models.Requests;

public class JiraIssuePostRequest
{
    public required string EpicKey { get; set; }
    public required string Summary { get; set; }
    public required string Description { get; set; }
    public string? Assignee { get; set; }
}