using Microsoft.EntityFrameworkCore;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using TL.ExemploCQRS.Infrastructure.Data;

namespace TL.ExemploCQRS.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        => await _dbSet.Where(p => p.IsActive).ToListAsync(cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Name == name);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}
