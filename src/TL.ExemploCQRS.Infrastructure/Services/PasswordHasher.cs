using System.Security.Cryptography;
using System.Text;
using TL.ExemploCQRS.Application.Interfaces;

namespace TL.ExemploCQRS.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        var result = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);

        return Convert.ToBase64String(result);
    }

    public bool Verify(string password, string hashString)
    {
        try
        {
            var bytes = Convert.FromBase64String(hashString);
            if (bytes.Length != SaltSize + HashSize) return false;

            var salt = new byte[SaltSize];
            var storedHash = new byte[HashSize];
            Buffer.BlockCopy(bytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(bytes, SaltSize, storedHash, 0, HashSize);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }
}
