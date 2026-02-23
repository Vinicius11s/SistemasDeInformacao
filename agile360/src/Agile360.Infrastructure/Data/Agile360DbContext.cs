using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agile360.Infrastructure.Data;

public class Agile360DbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public Agile360DbContext(DbContextOptions<Agile360DbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Advogado> Advogados => Set<Advogado>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Processo> Processos => Set<Processo>();
    public DbSet<Audiencia> Audiencias => Set<Audiencia>();
    public DbSet<Prazo> Prazos => Set<Prazo>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<EntradaIA> EntradasIA => Set<EntradaIA>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agile360DbContext).Assembly);

        if (_tenantProvider != null)
        {
            modelBuilder.Entity<Cliente>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Processo>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Audiencia>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Prazo>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Nota>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<EntradaIA>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
        }
    }
}
