namespace Blue.Mail2Epic.Dashboard.Models.Requests;

public sealed class UpdateUserAccountRequest
{
    public required bool IsActive { get; set; }
    public required bool IsAdmin { get; set; }
}

