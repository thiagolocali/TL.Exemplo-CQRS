using FluentAssertions;
using Moq;
using TL.ExemploCQRS.Application.Commands.Product.Create;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new CreateProductCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductAndReturnResponse()
    {
        // Arrange
        var command = new CreateProductCommand("Notebook Dell", "Descrição válida", 3500m, 10);

        _repositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.StockQuantity.Should().Be(command.StockQuantity);
        result.IsActive.Should().BeTrue();
        result.Id.Should().NotBeEmpty();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateProductCommand("Produto Duplicado", "Desc", 100m, 5);

        _repositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"*'{command.Name}'*");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
