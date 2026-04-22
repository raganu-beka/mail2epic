using Newtonsoft.Json;

namespace Blue.Mail2Epic.Models.Dtos.AzureAi;

public class AzureAiEmailAnalysisResponse
{
    [JsonProperty("requiresAction")] public bool RequiresAction { get; set; }
    [JsonProperty("reasoning")] public string? Reasoning { get; set; }
    [JsonProperty("epicKey")] public string? EpicKey { get; set; }
    [JsonProperty("epicConfidence")] public int? EpicConfidence { get; set; }
    [JsonProperty("suggestedTitle")] public string? SuggestedTitle { get; set; }
    [JsonProperty("suggestedDescription")] public string? SuggestedDescription { get; set; }
    [JsonProperty("suggestedPriority")] public string? SuggestedPriority { get; set; }
}
