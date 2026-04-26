using MediatR;
using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Interfaces;

namespace TL.ExemploCQRS.Application.Commands.Product.Delete;

// ── COMMAND ───────────────────────────────────────────────────────────────────
public record DeleteProductCommand(Guid Id) : IRequest;

// ── HANDLER ───────────────────────────────────────────────────────────────────
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        product.MarkAsDeleted();
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
