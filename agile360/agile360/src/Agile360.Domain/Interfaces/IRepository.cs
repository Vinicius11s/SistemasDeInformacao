using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

/// <summary>
/// Repositório genérico para persistência de entidades via Supabase Data API (PostgREST).
/// Cada operação envia Authorization: Bearer &lt;AccessToken&gt; ao PostgREST, garantindo RLS.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
}
