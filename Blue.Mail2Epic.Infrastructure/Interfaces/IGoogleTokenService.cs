namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IGoogleTokenService
{
    Task<(string Sub, string Email)> HandleAuthorizationCallbackAsync(
        string code,
        Func<string, int> resolveUserAccountId,
        CancellationToken ct = default);

    Task<string> GetValidAccessTokenAsync(int mailboxId, CancellationToken ct);
}
