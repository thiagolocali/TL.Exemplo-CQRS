using FluentAssertions;
using Moq;
using TL.ExemploCQRS.Application.Commands.Product.Delete;
using TL.ExemploCQRS.Application.Commands.Product.Update;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new UpdateProductCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProductAndReturnResponse()
    {
        // Arrange
        var existingProduct = Product.Create("Original", "Desc original", 100m, 5);
        var command = new UpdateProductCommand(existingProduct.Id, "Atualizado", "Nova desc", 200m, 15);

        _repositoryMock.Setup(r => r.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(command.Name, existingProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Atualizado");
        result.Price.Should().Be(200m);
        result.StockQuantity.Should().Be(15);
        result.UpdatedAt.Should().NotBeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await ((Func<Task>)(() => _handler.Handle(new UpdateProductCommand(id, "P", "D", 1m, 1), CancellationToken.None)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNameExistsOnOtherProduct_ShouldThrowDomainException()
    {
        // Arrange
        var existingProduct = Product.Create("Original", "Desc", 100m, 5);
        var command = new UpdateProductCommand(existingProduct.Id, "Nome Duplicado", "Desc", 100m, 5);

        _repositoryMock.Setup(r => r.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.ExistsByNameAsync(command.Name, existingProduct.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await ((Func<Task>)(() => _handler.Handle(command, CancellationToken.None)))
            .Should().ThrowAsync<DomainException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteProductCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldSoftDeleteProduct()
    {
        // Arrange
        var product = Product.Create("Produto", "Desc", 100m, 5);
        _repositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        product.IsDeleted.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await ((Func<Task>)(() => _handler.Handle(new DeleteProductCommand(id), CancellationToken.None)))
            .Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
