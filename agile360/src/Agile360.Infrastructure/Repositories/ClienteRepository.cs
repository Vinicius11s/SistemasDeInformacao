using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(Agile360DbContext context) : base(context) { }

    public async Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(c => c.CPF == cpf, ct);

    public async Task<IReadOnlyList<Cliente>> SearchAsync(string termo, CancellationToken ct = default)
    {
        var lower = termo.ToLower();
        return await _dbSet
            .Where(c => (c.NomeCompleto != null && c.NomeCompleto.ToLower().Contains(lower))
                     || (c.RazaoSocial  != null && c.RazaoSocial.ToLower().Contains(lower))
                     || (c.CPF          != null && c.CPF.Contains(lower))
                     || (c.Telefone     != null && c.Telefone.Contains(lower)))
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Cliente>> AddRangeAsync(
        IEnumerable<Cliente> clientes, CancellationToken ct = default)
    {
        var lista = clientes.ToList();
        if (lista.Count == 0) return [];

        foreach (var c in lista)
            if (c.Id == Guid.Empty) c.Id = Guid.NewGuid();

        await _dbSet.AddRangeAsync(lista, ct);
        await _context.SaveChangesAsync(ct);
        return lista;
    }
}
