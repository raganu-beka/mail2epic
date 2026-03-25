using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blue.Mail2Epic.Infrastructure.Models.Responses;

public class JiraIssueFields
{
    [JsonProperty("summary")] public string? Summary { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("updated")] public DateTimeOffset? UpdatedAt { get; set; }
    [JsonProperty("comment")] public JiraIssueComments? Comment { get; set; }
    [JsonProperty("reporter")] public JiraIssueReporter? Reporter { get; set; }
    
    [JsonExtensionData]
    public Dictionary<string, JToken?>? AdditionalFields { get; set; }
}