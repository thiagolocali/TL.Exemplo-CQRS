namespace TL.ExemploCQRS.Application.DTOs.Product;

// ── REQUEST DTOs ──────────────────────────────────────────────────────────────

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);

public record GetProductsRequest(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null
);

// ── RESPONSE DTOs ─────────────────────────────────────────────────────────────

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductSummaryResponse(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive
);
