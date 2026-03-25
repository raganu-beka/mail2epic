using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace Blue.Mail2Epic.Dashboard.Services;

public class LoginExchangeStore(IMemoryCache cache)
{
    private const string Prefix = "google-login-exchange:";

    public string Create(int userId)
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        var cacheKey = Prefix + token;

        cache.Set(
            cacheKey,
            userId,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            });

        return token;
    }

    public bool TryConsume(string exchangeToken, out int userId)
    {
        var cacheKey = Prefix + exchangeToken;

        if (cache.TryGetValue(cacheKey, out userId))
        {
            cache.Remove(cacheKey);
            return true;
        }

        userId = 0;
        return false;
    }
}