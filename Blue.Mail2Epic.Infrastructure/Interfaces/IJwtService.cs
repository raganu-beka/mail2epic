using Blue.Mail2Epic.Infrastructure.Models;

namespace Blue.Mail2Epic.Infrastructure.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserAccount user);
}
