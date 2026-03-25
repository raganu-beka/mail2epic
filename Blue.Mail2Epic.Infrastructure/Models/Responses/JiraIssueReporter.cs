using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Models.Responses;

public class JiraIssueReporter
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}