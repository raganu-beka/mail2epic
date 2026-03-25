using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Models.Responses;

public class JiraIssueResponse
{
    [JsonProperty("key")] public string? Key { get; set; }
    [JsonProperty("fields")] public JiraIssueFields? Fields { get; set; }
}
