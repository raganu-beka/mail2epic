namespace Blue.Mail2Epic.Models.Configuration;

public sealed class AzureAiOptions
{
    public const string SectionName = "AzureAi";
    public required string Endpoint { get; init; }
    public required string SummarizationModel { get; init; }
    public required string AnalysisModel { get; init; }
    public required string EmbeddingModel { get; init; }
    public required string ApiKey { get; init; }
    public int MaxConcurrentRequests { get; init; } = 1;
    public int NetworkTimeoutSeconds { get; init; } = 120;
}