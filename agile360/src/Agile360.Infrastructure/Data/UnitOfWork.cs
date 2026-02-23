using Agile360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly Agile360DbContext _context;

    public UnitOfWork(Agile360DbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
