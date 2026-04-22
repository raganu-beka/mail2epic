using Newtonsoft.Json;

namespace Blue.Mail2Epic.Models.Dtos.AzureAi;

public class AzureAiEpicFieldExtractionResponse
{
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("contactPersonEmail")] public string? ContactPersonEmail { get; set; }
}
