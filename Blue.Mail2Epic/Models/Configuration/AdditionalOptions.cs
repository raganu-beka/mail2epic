namespace Blue.Mail2Epic.Models.Configuration;

public sealed class AdditionalOptions
{
    public const string SectionName = "AdditionalOptions";
    public int? ReporterLookupIssueCount { get; init; }
}