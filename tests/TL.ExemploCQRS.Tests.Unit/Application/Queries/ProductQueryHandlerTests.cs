using FluentAssertions;
using Moq;
using TL.ExemploCQRS.Application.Queries.Product.GetAll;
using TL.ExemploCQRS.Application.Queries.Product.GetById;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Application.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ShouldReturnProductResponse()
    {
        // Arrange
        var product = Product.Create("Notebook", "Descrição do notebook", 3500m, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Price.Should().Be(product.Price);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await ((Func<Task>)(() => _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None)))
            .Should().ThrowAsync<NotFoundException>().WithMessage($"*'{id}'*");
    }

    [Fact]
    public async Task Handle_WhenProductIsDeleted_ShouldThrowNotFoundException()
    {
        // Arrange
        var product = Product.Create("Produto Deletado", "Desc", 100m, 5);
        product.MarkAsDeleted();
        _repositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act & Assert
        await ((Func<Task>)(() => _handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None)))
            .Should().ThrowAsync<NotFoundException>();
    }
}

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsQueryHandler(_repositoryMock.Object);
    }

    private static List<Product> CreateSampleProducts()
    {
        return new List<Product>
        {
            Product.Create("Notebook Dell", "Notebook potente", 3500m, 10),
            Product.Create("Mouse Logitech", "Mouse sem fio", 150m, 50),
            Product.Create("Teclado Mecânico", "Teclado gamer", 400m, 30),
            Product.Create("Monitor LG", "Monitor 4K", 2500m, 5)
        };
    }

    [Fact]
    public async Task Handle_WithDefaultQuery_ShouldReturnPagedResult()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleProducts());

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSearchFilter_ShouldReturnOnlyMatchingProducts()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleProducts());

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(Search: "Notebook"), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Contain("Notebook");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleProducts());

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoProducts_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var products = CreateSampleProducts();
        products[0].Deactivate();
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(IsActive: true), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task Handle_ShouldNotReturnDeletedProducts()
    {
        // Arrange
        var products = CreateSampleProducts();
        products[0].MarkAsDeleted();
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }
}
