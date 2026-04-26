using MediatR;
using Microsoft.AspNetCore.Mvc;
using TL.ExemploCQRS.Application.Commands.Auth;
using TL.ExemploCQRS.Application.Common;
using TL.ExemploCQRS.Application.DTOs.Auth;

namespace TL.ExemploCQRS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Autentica um usuário e retorna o token JWT</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login realizado com sucesso."));
    }

    /// <summary>Registra um novo usuário</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, ApiResponse<AuthResponse>.Ok(result, "Usuário registrado com sucesso."));
    }
}
