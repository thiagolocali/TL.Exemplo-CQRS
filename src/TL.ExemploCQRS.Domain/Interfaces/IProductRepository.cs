using TL.ExemploCQRS.Domain.Common;
using TL.ExemploCQRS.Domain.Entities;

namespace TL.ExemploCQRS.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Retorna uma página filtrada de produtos com contagem total.
    /// Filtros e paginação são executados no banco — nunca traz tudo para memória.
    /// O global QueryFilter do EF Core exclui automaticamente IsDeleted = true.
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Busca usuário ativo pelo username.
    /// Usuários com IsDeleted = true não devem ser retornados.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
