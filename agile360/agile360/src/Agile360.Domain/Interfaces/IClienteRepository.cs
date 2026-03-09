using Agile360.Domain.Entities;

namespace Agile360.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> SearchAsync(string termo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> AddRangeAsync(IEnumerable<Cliente> clientes, CancellationToken cancellationToken = default);
}
