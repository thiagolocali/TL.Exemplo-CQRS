using FluentAssertions;
using TL.ExemploCQRS.Domain.Entities;
using Xunit;

namespace TL.ExemploCQRS.Tests.Unit.Domain;

public class ProductEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProductCorrectly()
    {
        // Arrange
        var name = "Notebook Dell";
        var description = "Notebook Dell Inspiron 15";
        var price = 3500.00m;
        var stock = 10;

        // Act
        var product = Product.Create(name, description, price, stock);

        // Assert
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.StockQuantity.Should().Be(stock);
        product.IsActive.Should().BeTrue();
        product.IsDeleted.Should().BeFalse();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFieldsAndSetUpdatedAt()
    {
        // Arrange
        var product = Product.Create("Original", "Desc", 100m, 5);

        // Act
        product.Update("Atualizado", "Nova Desc", 200m, 15);

        // Assert
        product.Name.Should().Be("Atualizado");
        product.Description.Should().Be("Nova Desc");
        product.Price.Should().Be(200m);
        product.StockQuantity.Should().Be(15);
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedTrueAndUpdateTimestamp()
    {
        // Arrange
        var product = Product.Create("Produto", "Desc", 50m, 3);

        // Act
        product.MarkAsDeleted();

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var product = Product.Create("Produto", "Desc", 50m, 3);
        product.IsActive.Should().BeTrue();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var product = Product.Create("Produto", "Desc", 50m, 3);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EachProduct_ShouldHaveUniqueId()
    {
        // Act
        var p1 = Product.Create("P1", "D1", 10m, 1);
        var p2 = Product.Create("P2", "D2", 20m, 2);

        // Assert
        p1.Id.Should().NotBe(p2.Id);
    }
}

public class UserEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUserWithDefaultRole()
    {
        // Arrange & Act
        var user = User.Create("joao", "joao@email.com", "hash123");

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Username.Should().Be("joao");
        user.Email.Should().Be("joao@email.com");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be("User");
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithCustomRole_ShouldSetCorrectRole()
    {
        // Arrange & Act
        var user = User.Create("admin", "admin@email.com", "hash", "Admin");

        // Assert
        user.Role.Should().Be("Admin");
    }
}
