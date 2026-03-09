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
    public DbSet<Compromisso> Compromissos => Set<Compromisso>();
    public DbSet<Prazo> Prazos => Set<Prazo>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<RefreshTokenSession> RefreshTokenSessions => Set<RefreshTokenSession>();
    public DbSet<StagingCliente> StagingClientes => Set<StagingCliente>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();

    // Tabelas ainda não criadas no banco — DbSet disponível para migrations futuras,
    // mas as configurations abaixo mapeiam corretamente os nomes quando existirem.
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<EntradaIA> EntradasIA => Set<EntradaIA>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Agile360DbContext).Assembly);

        if (_tenantProvider != null)
        {
            // Apenas isolamento por tenant (AdvogadoId). Não usamos IsActive no filtro global porque
            // várias entidades não possuem coluna is_active mapeada (Processo, Prazo ignoram IsActive),
            // o que geraria "Translation of member 'IsActive' failed (unmapped)".
            // Filtro por ativo/inativo pode ser aplicado nos repositórios quando a tabela tiver a coluna.
            modelBuilder.Entity<Cliente>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Processo>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Compromisso>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Prazo>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<Nota>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
            modelBuilder.Entity<EntradaIA>().HasQueryFilter(e => e.AdvogadoId == _tenantProvider.GetCurrentAdvogadoId());
        }
    }
}
