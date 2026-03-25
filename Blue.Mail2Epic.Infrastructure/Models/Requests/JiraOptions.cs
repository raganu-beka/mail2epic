namespace Blue.Mail2Epic.Infrastructure.Models.Requests;

public class JiraOptions
{
    public const string SectionName = "Jira";
    public required string BaseUrl { get; set; }
    public required string AccessToken { get; set; }
    public required string ProjectKey { get; set; }
    public required string IssueType { get; set; }
    public required string EpicLinkFieldId { get; set; }
    public required string IssueLabel { get; set; }
}