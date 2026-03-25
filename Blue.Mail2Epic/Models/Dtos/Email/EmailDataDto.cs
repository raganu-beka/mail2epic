﻿namespace Blue.Mail2Epic.Models.Dtos.Email;

public class EmailDataDto
{
    public required string MessageId { get; init; }
    public required int UserAccountId { get; init; }
    public string? InReplyTo { get; init; }
    public List<string> References { get; init; } = [];
    public required string From { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required DateTimeOffset Date { get; init; }
    
    public string ThreadRootMessageId => References.FirstOrDefault() ?? InReplyTo ?? MessageId;
    public bool IsThreadRoot => ThreadRootMessageId == MessageId;
}
