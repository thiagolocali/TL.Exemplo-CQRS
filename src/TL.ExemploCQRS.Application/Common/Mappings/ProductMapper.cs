using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Domain.Entities;

namespace TL.ExemploCQRS.Application.Common.Mappings;

/// <summary>
/// Mapeamento manual entre entidades e DTOs — sem dependência de AutoMapper.
/// </summary>
public static class ProductMapper
{
    public static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt
    );

    public static ProductSummaryResponse ToSummaryResponse(Product product) => new(
        product.Id,
        product.Name,
        product.Price,
        product.StockQuantity,
        product.IsActive
    );
}
