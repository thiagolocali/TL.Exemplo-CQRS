using MediatR;
using TL.ExemploCQRS.Application.Common.Mappings;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Queries.Product.GetById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        // GetByIdAsync deve retornar null tanto para registros inexistentes
        // quanto para soft-deleted — responsabilidade centralizada no repositório.
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        return ProductMapper.ToResponse(product);
    }
}
