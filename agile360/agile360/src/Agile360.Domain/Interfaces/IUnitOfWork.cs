namespace Agile360.Domain.Interfaces;

/// <summary>
/// Unit of Work for transactional persistence. Implemented in Infrastructure.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
