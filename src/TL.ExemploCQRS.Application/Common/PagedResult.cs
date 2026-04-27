namespace TL.ExemploCQRS.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    // PageSize é garantido >= 1 pelo handler antes de chegar aqui,
    // mas a guard evita divisão por zero caso PagedResult seja construído diretamente.
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;

    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
