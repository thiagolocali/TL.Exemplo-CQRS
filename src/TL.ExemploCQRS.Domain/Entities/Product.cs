using TL.ExemploCQRS.Domain.Common;

namespace TL.ExemploCQRS.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private Product() { }

    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true
        };
    }

    public void Update(string name, string description, decimal price, int stockQuantity)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}
