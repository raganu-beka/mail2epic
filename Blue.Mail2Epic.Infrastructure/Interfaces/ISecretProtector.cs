namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface ISecretProtector
{
    string Protect(string value);
    string Unprotect(string value);
}
