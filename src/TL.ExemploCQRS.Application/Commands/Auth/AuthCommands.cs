using MediatR;
using TL.ExemploCQRS.Application.DTOs.Auth;
using TL.ExemploCQRS.Application.Interfaces;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Commands.Auth;

// ── LOGIN ─────────────────────────────────────────────────────────────────────
public record LoginCommand(string Username, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
            ?? throw new UnauthorizedException("Usuário ou senha inválidos.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Usuário ou senha inválidos.");

        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetTokenExpiration();

        return new AuthResponse(token, user.Username, user.Email, user.Role, expiresAt);
    }
}

// ── REGISTER ──────────────────────────────────────────────────────────────────
public record RegisterCommand(string Username, string Email, string Password) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
            throw new DomainException($"O nome de usuário '{request.Username}' já está em uso.");

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail != null)
            throw new DomainException("Este e-mail já está cadastrado.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Username, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetTokenExpiration();

        return new AuthResponse(token, user.Username, user.Email, user.Role, expiresAt);
    }
}
