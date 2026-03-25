namespace Blue.Mail2Epic.Models.Configuration;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public required string Host { get; init; }
    public int Port { get; init; }
    public int MaxEmailsPerHour { get; init; } = 50;
    public int MaxEmailsAbsoluteCap { get; init; } = 500;
    public int DelayBetweenInboxesMs { get; init; } = 500;
    public List<string> IgnoredEmailAddresses { get; init; } = [];
}