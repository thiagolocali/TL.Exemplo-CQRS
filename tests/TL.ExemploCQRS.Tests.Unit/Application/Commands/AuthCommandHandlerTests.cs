using FluentAssertions;
using Moq;
using TL.ExemploCQRS.Application.Commands.Auth;
using TL.ExemploCQRS.Application.Interfaces;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new LoginCommandHandler(_userRepoMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = User.Create("joao", "joao@email.com", "hashValido", "User");
        var command = new LoginCommand("joao", "Senha@123");
        var expectedToken = "jwt.token.aqui";
        var expectedExpiry = DateTime.UtcNow.AddHours(8);

        _userRepoMock
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(user))
            .Returns(expectedToken);

        _jwtServiceMock
            .Setup(j => j.GetTokenExpiration())
            .Returns(expectedExpiry);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role);
        result.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new LoginCommand("naoexiste", "qualquer"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Usuário ou senha inválidos.");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = User.Create("joao", "joao@email.com", "hashValido");

        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("joao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify("senhaErrada", user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _handler.Handle(new LoginCommand("joao", "senhaErrada"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Usuário ou senha inválidos.");
    }
}

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new RegisterCommandHandler(_userRepoMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var command = new RegisterCommand("novousuario", "novo@email.com", "Senha@123");

        _userRepoMock.Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(h => h.Hash(command.Password)).Returns("hashGerado");
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token.gerado");
        _jwtServiceMock.Setup(j => j.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(8));
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("token.gerado");
        result.Username.Should().Be(command.Username);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUsernameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var existingUser = User.Create("duplicado", "outro@email.com", "hash");
        var command = new RegisterCommand("duplicado", "novo@email.com", "Senha@123");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("duplicado", It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*'duplicado'*");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterCommand("novousuario", "existente@email.com", "Senha@123");
        var existingUser = User.Create("outro", "existente@email.com", "hash");

        _userRepoMock.Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*e-mail*");
    }
}
