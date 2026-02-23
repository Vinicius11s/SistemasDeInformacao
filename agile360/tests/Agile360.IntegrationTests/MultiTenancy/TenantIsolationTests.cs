using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;
using Agile360.Infrastructure.Data.Interceptors;
using Agile360.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Agile360.IntegrationTests.MultiTenancy;

/// <summary>
/// Phase 4: Multi-Tenancy isolation tests. Uses SQLite in-memory + real DbContext with query filters and interceptor.
/// </summary>
public class TenantIsolationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestTenantProvider _tenantProvider;

    public TenantIsolationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _tenantProvider = new TestTenantProvider();
    }

    private Agile360DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Agile360DbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(
                new TenantSaveChangesInterceptor(_tenantProvider),
                new AuditSaveChangesInterceptor(_tenantProvider))
            .Options;
        return new Agile360DbContext(options, _tenantProvider);
    }

    private static void EnsureCreatedAndSeed(Agile360DbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Advogados.Any())
            return;

        var advogadoA = new Advogado
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nome = "Advogado A",
            Email = "a@test.com",
            OAB = "OAB/SP 111111",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var advogadoB = new Advogado
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nome = "Advogado B",
            Email = "b@test.com",
            OAB = "OAB/SP 222222",
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Advogados.AddRange(advogadoA, advogadoB);

        var clienteA = new Cliente
        {
            Id = Guid.NewGuid(),
            AdvogadoId = advogadoA.Id,
            Nome = "Cliente de A",
            Origem = OrigemCliente.Manual,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var clienteB = new Cliente
        {
            Id = Guid.NewGuid(),
            AdvogadoId = advogadoB.Id,
            Nome = "Cliente de B",
            Origem = OrigemCliente.Manual,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Clientes.AddRange(clienteA, clienteB);

        var processoA = new Processo
        {
            Id = Guid.NewGuid(),
            AdvogadoId = advogadoA.Id,
            ClienteId = clienteA.Id,
            NumeroProcesso = "0000000-00.2024.8.26.0100",
            Status = StatusProcesso.Ativo,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var processoB = new Processo
        {
            Id = Guid.NewGuid(),
            AdvogadoId = advogadoB.Id,
            ClienteId = clienteB.Id,
            NumeroProcesso = "0000000-00.2024.8.26.0200",
            Status = StatusProcesso.Ativo,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Processos.AddRange(processoA, processoB);
        context.SaveChanges();
    }

    [Fact]
    public void QueryFilter_AdvogadoA_DoesNotSeeClientesOfAdvogadoB()
    {
        using var context = CreateContext();
        EnsureCreatedAndSeed(context);

        _tenantProvider.SetCurrentAdvogadoId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var clientes = context.Clientes.ToList();

        clientes.Should().HaveCount(1);
        clientes[0].Nome.Should().Be("Cliente de A");
        clientes[0].AdvogadoId.Should().Be(_tenantProvider.GetCurrentAdvogadoId());
    }

    [Fact]
    public void QueryFilter_AdvogadoA_ProcessosListIsIsolatedByTenant()
    {
        using var context = CreateContext();
        EnsureCreatedAndSeed(context);

        _tenantProvider.SetCurrentAdvogadoId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var processos = context.Processos.ToList();

        processos.Should().HaveCount(1);
        processos[0].AdvogadoId.Should().Be(_tenantProvider.GetCurrentAdvogadoId());
        processos[0].NumeroProcesso.Should().Contain("0100");
    }

    [Fact]
    public void Insert_NewClienteWithoutAdvogadoId_ReceivesCurrentTenantFromInterceptor()
    {
        using var context = CreateContext();
        EnsureCreatedAndSeed(context);

        var advogadoAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        _tenantProvider.SetCurrentAdvogadoId(advogadoAId);

        var novoCliente = new Cliente
        {
            Id = Guid.NewGuid(),
            AdvogadoId = Guid.Empty,
            Nome = "Novo Cliente",
            Origem = OrigemCliente.Manual,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Clientes.Add(novoCliente);
        context.SaveChanges();

        novoCliente.AdvogadoId.Should().Be(advogadoAId);

        var fromDb = context.Clientes.First(c => c.Id == novoCliente.Id);
        fromDb.AdvogadoId.Should().Be(advogadoAId);
    }

    [Fact]
    public void Insert_ProcessoWithTenant_FillsCreatedByAndLastModifiedByAndWritesAuditLog()
    {
        using var context = CreateContext();
        EnsureCreatedAndSeed(context);

        var advogadoAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        _tenantProvider.SetCurrentAdvogadoId(advogadoAId);

        var clienteAId = context.Clientes
            .IgnoreQueryFilters()
            .First(c => c.AdvogadoId == advogadoAId).Id;

        var processoId = Guid.NewGuid();
        var novoProcesso = new Processo
        {
            Id = processoId,
            AdvogadoId = advogadoAId,
            ClienteId = clienteAId,
            NumeroProcesso = "0000000-00.2024.8.26.0300",
            Status = StatusProcesso.Ativo,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Processos.Add(novoProcesso);
        context.SaveChanges();

        var createdBy = context.Processos
            .IgnoreQueryFilters()
            .Where(p => p.Id == processoId)
            .Select(p => EF.Property<Guid?>(p, "CreatedBy"))
            .FirstOrDefault();
        var lastModifiedBy = context.Processos
            .IgnoreQueryFilters()
            .Where(p => p.Id == processoId)
            .Select(p => EF.Property<Guid?>(p, "LastModifiedBy"))
            .FirstOrDefault();

        createdBy.Should().Be(advogadoAId);
        lastModifiedBy.Should().Be(advogadoAId);

        var auditLog = context.AuditLogs
            .IgnoreQueryFilters()
            .FirstOrDefault(al => al.EntityName == "Processo" && al.EntityId == processoId);
        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be(AuditAction.Created);
        auditLog.AdvogadoId.Should().Be(advogadoAId);
        auditLog.NewValues.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_ClienteOfOtherTenant_ReturnsNull()
    {
        var advogadoBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        using var context = CreateContext();
        EnsureCreatedAndSeed(context);

        Guid clienteBId = context.Clientes
            .IgnoreQueryFilters()
            .First(c => c.AdvogadoId == advogadoBId).Id;

        _tenantProvider.SetCurrentAdvogadoId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var repository = new ClienteRepository(context);
        var result = await repository.GetByIdAsync(clienteBId);

        result.Should().BeNull();
    }

    public void Dispose() => _connection.Dispose();
}
