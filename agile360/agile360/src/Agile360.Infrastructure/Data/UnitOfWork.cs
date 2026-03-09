using Agile360.Domain.Interfaces;

namespace Agile360.Infrastructure.Data;

/// <summary>
/// Unit of Work adaptado para PostgREST: cada operação de repositório é imediatamente
/// commitada via HTTP, portanto SaveChangesAsync é um no-op (retorna 0).
/// Mantido para compatibilidade de interface com a camada de Aplicação.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public void Dispose() { }
}
