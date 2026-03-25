using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Models.Responses;

public class JiraIssueComments
{
    [JsonProperty("comments")] public List<JiraComment>? Comments { get; set; }
}

public class JiraComment
{
    [JsonProperty("body")] public string? Body { get; set; }
    [JsonProperty("updated")] public DateTimeOffset? UpdatedAt { get; set; }
}