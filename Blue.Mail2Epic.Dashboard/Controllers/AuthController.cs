using System.Security.Cryptography;
using Blue.Mail2Epic.Dashboard.Models;
using Blue.Mail2Epic.Dashboard.Models.Requests;
using Blue.Mail2Epic.Dashboard.Services;
using Blue.Mail2Epic.Infrastructure.Data;
using Blue.Mail2Epic.Infrastructure.Models;
using Blue.Mail2Epic.Infrastructure.Models.Configuration;
using Blue.Mail2Epic.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blue.Mail2Epic.Dashboard.Controllers;

[ApiController]
[Route("api/auth/google")]
public class AuthController(
    IOptions<GoogleOAuthOptions> oAuthOptions,
    IOptions<FrontendOptions> frontendOptions,
    AppDbContext dbContext,
    LoginExchangeStore exchangeStore,
    JwtService jwtService,
    GoogleTokenService googleTokenService
    ) : ControllerBase
{
    private readonly GoogleOAuthOptions _oAuthOptions = oAuthOptions.Value;
    private readonly FrontendOptions _frontendOptions = frontendOptions.Value;
    
    [HttpGet("start")]
    public IActionResult Start()
    {
        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = WebEncoders.Base64UrlEncode(stateBytes);
        
        Response.Cookies.Append("google_oauth_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });
        
        var url = QueryHelpers.AddQueryString(_oAuthOptions.AuthorizationEndpoint, new Dictionary<string, string?>
        {
            ["client_id"] = _oAuthOptions.ClientId,
            ["redirect_uri"] = _oAuthOptions.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _oAuthOptions.Scope,
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["include_granted_scopes"] = "true",
            ["state"] = state
        });

        return Redirect(url);
    }
    
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken ct)
    {
        var cookieState = Request.Cookies["google_oauth_state"];
        if (string.IsNullOrWhiteSpace(cookieState) || cookieState != state)
        {
            return BadRequest("Invalid OAuth state.");
        }

        string email;
        try
        {
            (_, email) = await googleTokenService.HandleAuthorizationCallbackAsync(code, ResolveUserAccountId, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, "User does not have an allowed account.");
        }

        var userAccount = GetUserAccount(email);
        if (userAccount is null) return StatusCode(403, "User does not have an allowed account.");

        var exchange = exchangeStore.Create(userAccount.Id);

        return Redirect($"{_frontendOptions.BaseUrl}{_frontendOptions.LoginExchangeEndpoint}?exchange={exchange}");
    }
    
    [HttpPost("exchange-google-login")]
    public async Task<IActionResult> ExchangeGoogleLogin([FromBody] ExchangeLoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Exchange)) return BadRequest();

        if (!exchangeStore.TryConsume(request.Exchange, out var userId)) return Unauthorized();

        var user = await dbContext.UserAccounts.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return Unauthorized();

        var token = jwtService.GenerateToken(user);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return Ok(new { token });
    }
    
    private int ResolveUserAccountId(string email)
    {
        var user = GetUserAccount(email);
        return user?.Id ?? throw new UnauthorizedAccessException();
    }
    
    private UserAccount? GetUserAccount(string email)
    {
        var user = dbContext.UserAccounts.FirstOrDefault(x => x.Email == email);
        return user is not null && user.IsActive ? user : null;
    }
}