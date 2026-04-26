using MediatR;
using TL.ExemploCQRS.Application.Common;
using TL.ExemploCQRS.Application.Common.Mappings;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Queries.Product.GetAll;

public record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null
) : IRequest<PagedResult<ProductSummaryResponse>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductSummaryResponse>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductSummaryResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var allProducts = await _repository.GetAllAsync(cancellationToken);

        var query = allProducts.Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase));

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        var totalCount = query.Count();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProductMapper.ToSummaryResponse)
            .ToList();

        return new PagedResult<ProductSummaryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
