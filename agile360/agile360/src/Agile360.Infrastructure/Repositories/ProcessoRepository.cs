using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Enums;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;

namespace Agile360.Infrastructure.Repositories;

public class ProcessoRepository : Repository<Processo>, IProcessoRepository
{
    public ProcessoRepository(SupabaseDataClient client, ICurrentUserService currentUser)
        : base(client, currentUser) { }

    /// <summary>
    /// Override obrigatório: a coluna criado_em é NOT NULL sem DEFAULT no Supabase.
    /// Sem este override, WhenWritingNull omite o campo do JSON e o banco retorna 23502.
    /// </summary>
    public override async Task<Processo> AddAsync(Processo entity, CancellationToken ct = default)
    {
        entity.CriadoEm ??= DateOnly.FromDateTime(DateTime.UtcNow);
        return await base.AddAsync(entity, ct);
    }

    // GET /rest/v1/processo?num_processo=eq.{numero}&limit=1
    public Task<Processo?> GetByNumeroAsync(string numero, CancellationToken ct = default) =>
        _client.GetSingleAsync<Processo>(TableName,
            $"num_processo=eq.{Uri.EscapeDataString(numero)}", Token, ct);

    // GET /rest/v1/processo?order=criado_em.desc,id.desc&limit={count}
    public Task<IReadOnlyList<Processo>> GetRecentesAsync(int count, CancellationToken ct = default) =>
        _client.GetListAsync<Processo>(TableName,
            $"order=criado_em.desc,id.desc&limit={count}", Token, ct);

    // GET /rest/v1/processo?status=eq.{status}
    public Task<IReadOnlyList<Processo>> GetByStatusAsync(StatusProcesso status, CancellationToken ct = default) =>
        _client.GetListAsync<Processo>(TableName,
            $"status=eq.{status}", Token, ct);
}
