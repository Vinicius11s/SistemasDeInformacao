using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Repositories;

/// <summary>
/// Repositório genérico baseado em EF Core.
/// Multi-tenancy via Global Query Filter no DbContext (AdvogadoId + IsActive).
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly Agile360DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(Agile360DbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.AsNoTracking().Take(500).ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task RemoveAsync(T entity, CancellationToken ct = default)
    {
        entity.IsActive = false;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }
}
