﻿using System.ComponentModel.DataAnnotations;

 namespace Blue.Mail2Epic.Infrastructure.Models;

public class UserAccount
{
    public int Id { get; set; }
    [MaxLength(200)] public required string Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    
    public GoogleMailboxAccount? GoogleMailboxAccount { get; set; }
    public ICollection<EmailMappingRecipient> EmailMappingRecipients { get; set; } = [];
}