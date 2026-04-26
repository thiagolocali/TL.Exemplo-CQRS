using FluentAssertions;
using FluentValidation.TestHelper;
using TL.ExemploCQRS.Application.DTOs.Auth;
using TL.ExemploCQRS.Application.Validators.Auth;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Validators;

public class LoginValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidCredentials_ShouldPassValidation()
    {
        var request = new LoginRequest("usuario", "senha123");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUsername_ShouldFailValidation()
    {
        var request = new LoginRequest("", "senha123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFailValidation()
    {
        var request = new LoginRequest("usuario", "");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public class RegisterValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPassValidation()
    {
        var request = new RegisterRequest("joao_silva", "joao@email.com", "Senha@123", "Senha@123");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("senha")]         // sem maiúscula, número, especial
    [InlineData("Senha123")]      // sem especial
    [InlineData("SENHA@123")]     // sem minúscula
    [InlineData("Senha@abc")]     // sem número
    [InlineData("S@1a")]          // menos de 8 chars
    public void Validate_WithWeakPassword_ShouldFailValidation(string password)
    {
        var request = new RegisterRequest("usuario", "user@email.com", password, password);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordMismatch_ShouldFailOnConfirmPassword()
    {
        var request = new RegisterRequest("usuario", "user@email.com", "Senha@123", "Diferente@123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("As senhas não coincidem.");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFailValidation()
    {
        var request = new RegisterRequest("usuario", "emailinvalido", "Senha@123", "Senha@123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("us")]            // menos de 3 chars
    [InlineData("usuario invalido")] // tem espaço
    [InlineData("usuario@especial")] // tem @
    public void Validate_WithInvalidUsername_ShouldFailValidation(string username)
    {
        var request = new RegisterRequest(username, "user@email.com", "Senha@123", "Senha@123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
