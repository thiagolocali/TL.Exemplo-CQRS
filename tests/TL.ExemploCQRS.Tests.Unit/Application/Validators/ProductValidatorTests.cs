using FluentAssertions;
using FluentValidation.TestHelper;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Application.Validators.Product;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPassValidation()
    {
        var request = new CreateProductRequest("Notebook Dell", "Descrição válida do produto", 3500m, 10);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void Validate_WithInvalidName_ShouldFailValidation(string name)
    {
        var request = new CreateProductRequest(name, "Descrição válida", 100m, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding150Chars_ShouldFailValidation()
    {
        var name = new string('A', 151);
        var request = new CreateProductRequest(name, "Descrição válida", 100m, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Validate_WithPriceZeroOrNegative_ShouldFailValidation(decimal price)
    {
        var request = new CreateProductRequest("Produto Válido", "Descrição válida", price, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WithPriceAboveMax_ShouldFailValidation()
    {
        var request = new CreateProductRequest("Produto", "Descrição válida", 1_000_000m, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WithNegativeStock_ShouldFailValidation()
    {
        var request = new CreateProductRequest("Produto", "Descrição válida", 100m, -1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity);
    }

    [Fact]
    public void Validate_WithZeroStock_ShouldPassValidation()
    {
        var request = new CreateProductRequest("Produto Válido", "Descrição válida do produto", 100m, 0);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.StockQuantity);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldFailValidation()
    {
        var request = new CreateProductRequest("Produto Válido", "", 100m, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}

public class UpdateProductValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPassValidation()
    {
        var request = new UpdateProductRequest("Produto Atualizado", "Descrição atualizada", 250m, 20);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidName_ShouldReturnCorrectMessage()
    {
        var request = new UpdateProductRequest("ab", "Descrição válida", 100m, 5);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("O nome deve ter no mínimo 3 caracteres.");
    }
}
