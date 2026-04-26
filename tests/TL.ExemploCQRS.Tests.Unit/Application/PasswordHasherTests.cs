using FluentAssertions;
using TL.ExemploCQRS.Infrastructure.Services;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var hash = _hasher.Hash("Senha@123");
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_SamePassword_ShouldReturnDifferentHashes()
    {
        // PBKDF2 usa salt aleatório — dois hashes da mesma senha devem ser diferentes
        var hash1 = _hasher.Hash("Senha@123");
        var hash2 = _hasher.Hash("Senha@123");
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "Senha@123";
        var hash = _hasher.Hash(password);
        _hasher.Verify(password, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ShouldReturnFalse()
    {
        var hash = _hasher.Hash("Senha@123");
        _hasher.Verify("SenhaErrada@456", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_WithTamperedHash_ShouldReturnFalse()
    {
        _hasher.Verify("Senha@123", "hashInvalido").Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyHash_ShouldReturnFalse()
    {
        _hasher.Verify("Senha@123", "").Should().BeFalse();
    }
}
