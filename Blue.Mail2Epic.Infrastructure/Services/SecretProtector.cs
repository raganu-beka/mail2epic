using Blue.Mail2Epic.Infrastructure.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Blue.Mail2Epic.Infrastructure.Services;

public sealed class SecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("google-mailbox-tokens");
    
    public string Protect(string value) => _protector.Protect(value);
    public string Unprotect(string value) => _protector.Unprotect(value);
}
