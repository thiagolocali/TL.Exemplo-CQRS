using TL.ExemploCQRS.Domain.Common;

namespace TL.ExemploCQRS.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User";

    private User() { }

    public static User Create(string username, string email, string passwordHash, string role = "User")
    {
        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
