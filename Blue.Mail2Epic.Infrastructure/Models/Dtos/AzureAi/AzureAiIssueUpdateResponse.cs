using Newtonsoft.Json;

namespace Blue.Mail2Epic.Models.Dtos.AzureAi;

public class AzureAiIssueUpdateResponse
{
    [JsonProperty("hasUpdate")] public bool RequiresAction { get; set; }
    [JsonProperty("reasoning")] public string? Reasoning { get; set; }
    [JsonProperty("comment")] public string? Comment { get; set; }
}
