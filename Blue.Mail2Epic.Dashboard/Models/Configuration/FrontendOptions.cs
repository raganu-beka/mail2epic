namespace Blue.Mail2Epic.Dashboard.Models;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";
    public required string BaseUrl { get; set; }
    public required string LoginExchangeEndpoint { get; set; }
    public required string MessageSenderRegex { get; set; }
    public required string JiraIssueLinkTemplate { get; set; }
    public required string GmailEmailLinkTemplate { get; set; }
}