namespace TL.ExemploCQRS.Application.DTOs.Auth;

public record LoginRequest(
    string Username,
    string Password
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
);

public record AuthResponse(
    string Token,
    string Username,
    string Email,
    string Role,
    DateTime ExpiresAt
);
