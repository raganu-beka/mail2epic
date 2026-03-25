using System.ComponentModel.DataAnnotations;

namespace Blue.Mail2Epic.Dashboard.Models.Requests;

public sealed class CreateAllowedAccountRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public required string Email { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsAdmin { get; set; } = false;
}

