using System.ComponentModel.DataAnnotations;

namespace Blue.Mail2Epic.Infrastructure.Models;

public enum RequiredAction
{
    AppendExistingIssue,
    CreateNewIssue,
    NoAction
}

public class EmailMapping
{
    public int Id { get; set; }
    
    [MaxLength(200)] public required string MessageSenderAddress { get; set; }
    [MaxLength(200)] public required string MessageSubject { get; set; }
    [MaxLength(1000)] public required string MessageId { get; set; }
    [MaxLength(1000)] public required string ThreadRootMessageId { get; set; }
    
    [MaxLength(50)] public string? JiraIssueKey { get; set; }
    public required RequiredAction ActionTaken { get; set; } 
    [MaxLength(500)] public string? Reasoning { get; set; }
    [MaxLength(50)] public string? SuggestedEpicKey { get; set; }
    public int? EpicConfidence { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    public ICollection<EmailMappingRecipient> Recipients { get; set; } = [];
}