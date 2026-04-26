using TL.ExemploCQRS.Domain.Entities;

namespace TL.ExemploCQRS.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetTokenExpiration();
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
