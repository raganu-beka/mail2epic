using System.IdentityModel.Tokens.Jwt;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Configuration;
using Blue.Mail2Epic.Infrastructure.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blue.Mail2Epic.Infrastructure.Services;

public class GoogleTokenService(
    IOptions<GoogleOAuthOptions> options,
    IHttpClientFactory httpClientFactory,
    AppDbContext dbContext,
    SecretProtector protector
)
{
    private readonly GoogleOAuthOptions _options = options.Value;
    
    public async Task<(string Sub, string Email)> HandleAuthorizationCallbackAsync(
        string code,
        Func<string, int> resolveUserAccountId,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        });

        using var client = httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Google token exchange failed. Status={(int)response.StatusCode}, Body={json}");

        var token = JsonConvert.DeserializeObject<GoogleTokenResponse>(json)
                    ?? throw new InvalidOperationException("Empty token response from Google.");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.IdToken);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                  ?? throw new InvalidOperationException("Missing 'sub' claim in Google id_token.");
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                    ?? throw new InvalidOperationException("Missing 'email' claim in Google id_token.");
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

        await UpsertMailboxAccountAsync(sub, email, name, token, resolveUserAccountId(email), ct);

        return (sub, email);
    }

    private async Task UpsertMailboxAccountAsync(
        string sub, string email, string name, GoogleTokenResponse token, int userAccountId, CancellationToken ct)
    {
        var existing = await dbContext.GoogleMailboxAccounts
            .FirstOrDefaultAsync(x => x.GoogleSubject == sub, ct);

        if (existing is null)
        {
            dbContext.GoogleMailboxAccounts.Add(new GoogleMailboxAccount
            {
                UserAccountId = userAccountId,
                GoogleSubject = sub,
                EmailAddress = email,
                DisplayName = name,
                EncryptedRefreshToken = protector.Protect(token.RefreshToken ?? ""),
                EncryptedAccessToken = protector.Protect(token.AccessToken),
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn),
                Scope = token.Scope,
                TokenType = token.TokenType,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            existing.EmailAddress = email;
            existing.DisplayName = name;

            if (!string.IsNullOrWhiteSpace(token.RefreshToken))
                existing.EncryptedRefreshToken = protector.Protect(token.RefreshToken);

            existing.EncryptedAccessToken = protector.Protect(token.AccessToken);
            existing.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
            existing.Scope = token.Scope;
            existing.TokenType = token.TokenType;
            existing.ModifiedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<string> GetValidAccessTokenAsync(int mailboxId, CancellationToken ct)
    {
        var mailbox = await dbContext.GoogleMailboxAccounts.SingleAsync(x => x.Id == mailboxId, ct);

        var now = DateTimeOffset.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(mailbox.EncryptedAccessToken) &&
            mailbox.AccessTokenExpiresAt is { } expiresAt &&
            expiresAt > now.AddMinutes(5))
        {
            return protector.Unprotect(mailbox.EncryptedAccessToken);
        }

        return await RefreshAccessTokenAsync(mailbox, ct);
    }
    
    private async Task<string> RefreshAccessTokenAsync(GoogleMailboxAccount mailbox, CancellationToken ct)
    {
        var refreshToken = protector.Unprotect(mailbox.EncryptedRefreshToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        using var client = httpClientFactory.CreateClient();

        using var response = await client.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google token refresh failed. Status={(int)response.StatusCode}, Body={json}");
        }

        var tokenResponse = JsonConvert.DeserializeObject<GoogleRefreshTokenResponse>(json)
                            ?? throw new InvalidOperationException("Google token refresh returned an invalid response.");

        mailbox.EncryptedAccessToken = protector.Protect(tokenResponse.AccessToken);
        mailbox.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        mailbox.ModifiedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return tokenResponse.AccessToken;
    }
}