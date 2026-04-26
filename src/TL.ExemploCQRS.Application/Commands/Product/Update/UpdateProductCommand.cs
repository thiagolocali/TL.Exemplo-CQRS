using MediatR;
using TL.ExemploCQRS.Application.Common.Mappings;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Commands.Product.Update;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
) : IRequest<ProductResponse>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IProductRepository _repository;

    public UpdateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        var nameExists = await _repository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (nameExists)
            throw new DomainException($"Já existe outro produto com o nome '{request.Name}'.");

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToResponse(product);
    }
}
