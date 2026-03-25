using System.ComponentModel.DataAnnotations;

namespace Blue.Mail2Epic.Infrastructure.Models;

public class GoogleMailboxAccount
{
    public int Id { get; set; }
    public int UserAccountId { get; set; }
    
    [MaxLength(1000)] public required string GoogleSubject { get; set; }
    [MaxLength(200)] public required string EmailAddress { get; set; }
    [MaxLength(200)] public required string DisplayName { get; set; }
    
    public required string EncryptedRefreshToken { get; set; }
    public string? EncryptedAccessToken { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    
    [MaxLength(2000)] public required string Scope { get; set; }
    [MaxLength(50)]  public required string TokenType { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    
    public UserAccount UserAccount { get; set; } = null!;
}