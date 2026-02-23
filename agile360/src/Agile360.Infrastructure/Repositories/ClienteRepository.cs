using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(Agile360DbContext context) : base(context)
    {
    }

    public async Task<Cliente?> GetByWhatsAppAsync(string numero, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(c => c.WhatsAppNumero == numero, cancellationToken);

    public async Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(c => c.CPF == cpf, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> SearchAsync(string termo, CancellationToken cancellationToken = default)
    {
        var lower = termo.ToLowerInvariant();
        return await _dbSet
            .Where(c => (c.Nome != null && c.Nome.ToLower().Contains(lower)) ||
                        (c.CPF != null && c.CPF.Contains(termo)) ||
                        (c.Telefone != null && c.Telefone.Contains(termo)) ||
                        (c.Email != null && c.Email.ToLower().Contains(lower)))
            .ToListAsync(cancellationToken);
    }
}
