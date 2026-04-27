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

    public async Task<PagedResult<ProductSummaryResponse>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Garante valores válidos antes de enviar ao repositório
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Filtro e paginação executados no banco via repositório
        var (products, totalCount) = await _repository.GetPagedAsync(
            page,
            pageSize,
            request.Search,
            request.IsActive,
            cancellationToken);

        var items = products
            .Select(ProductMapper.ToSummaryResponse)
            .ToList();

        return new PagedResult<ProductSummaryResponse>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
