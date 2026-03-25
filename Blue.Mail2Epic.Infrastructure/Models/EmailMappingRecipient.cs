namespace Blue.Mail2Epic.Infrastructure.Models;

public class EmailMappingRecipient
{
    public int EmailMappingId { get; set; }
    public int UserAccountId { get; set; }
    public EmailMapping EmailMapping { get; set; } = null!;
    public UserAccount UserAccount { get; set; } = null!;
}