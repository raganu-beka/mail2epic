using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Models.Responses;

public class JiraSearchResponse
{
    [JsonProperty("issues")] public List<JiraIssueResponse>? Issues { get; set; }
}