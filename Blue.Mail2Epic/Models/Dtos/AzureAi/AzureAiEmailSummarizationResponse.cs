using Newtonsoft.Json;

namespace Blue.Mail2Epic.Models.Dtos.Email;

public class AzureAiEmailSummarizationResponse
{
    [JsonProperty("subject")] public string? Subject { get; set; }
    [JsonProperty("summary")] public string? Summary { get; set; }
    [JsonProperty("participants")] public List<EmailParticipant> Participants { get; set; } = [];
    [JsonProperty("keyEntities")] public List<string>? KeyEntities { get; set; }
}

public class EmailParticipant
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("email")] public string? Email { get; set; }
}