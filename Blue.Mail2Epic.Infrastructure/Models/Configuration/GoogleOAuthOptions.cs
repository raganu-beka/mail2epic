namespace Blue.Mail2Epic.Infrastructure.Models.Configuration;

public sealed class GoogleOAuthOptions
{
    public const string SectionName = "GoogleOAuth";
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RedirectUri { get; set; }
    public required string Scope { get; set; }
    public required string AuthorizationEndpoint { get; set; }
    public required string TokenEndpoint { get; set; }
}
