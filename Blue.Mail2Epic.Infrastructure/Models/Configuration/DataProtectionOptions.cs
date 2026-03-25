namespace Blue.Mail2Epic.Infrastructure.Models.Configuration;

public class DataProtectionOptions
{
    public const string SectionName = "DataProtection";
    public required string KeyStoragePath { get; set; }
}