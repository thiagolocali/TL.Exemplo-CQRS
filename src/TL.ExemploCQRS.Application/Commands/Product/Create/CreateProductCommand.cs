using MediatR;
using TL.ExemploCQRS.Application.Common.Mappings;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Commands.Product.Create;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
) : IRequest<ProductResponse>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _repository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken);
        if (nameExists)
            throw new DomainException($"Já existe um produto com o nome '{request.Name}'.");

        var product = Domain.Entities.Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToResponse(product);
    }
}
