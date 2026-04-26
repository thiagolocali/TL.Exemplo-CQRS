using Microsoft.EntityFrameworkCore;
using TL.ExemploCQRS.Domain.Entities;
using TL.ExemploCQRS.Domain.Interfaces;
using TL.ExemploCQRS.Infrastructure.Data;

namespace TL.ExemploCQRS.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
