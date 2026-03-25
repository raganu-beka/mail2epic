namespace Blue.Mail2Epic.Models.Configuration;

public class PromptOptions
{
    public const string SectionName = "PromptOptions";
    public required string EpicFieldExtractionPrompt { get; init; }
    public required string EmailSummarizationPrompt { get; init; }
    public required string EmailTriagePrompt { get; init; }
    public required string IssueUpdatePrompt { get; init; }
}