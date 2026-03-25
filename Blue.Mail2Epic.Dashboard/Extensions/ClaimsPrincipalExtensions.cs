using System.Security.Claims;

namespace Blue.Mail2Epic.Dashboard.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(identifier, out var id) ? id : null;
    }
}

