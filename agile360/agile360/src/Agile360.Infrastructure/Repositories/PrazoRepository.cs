using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;

namespace Agile360.Infrastructure.Repositories;

public class PrazoRepository : Repository<Prazo>, IPrazoRepository
{
    // A tabela no Supabase chama-se "prazo" (singular) — coincide com
    // typeof(Prazo).Name.ToLowerInvariant() → sem necessidade de override do TableName.

    public PrazoRepository(SupabaseDataClient client, ICurrentUserService currentUser)
        : base(client, currentUser) { }

    // ─── Consultas especializadas ─────────────────────────────────────────────

    /// <summary>
    /// GET prazos Pendentes vencendo em até <paramref name="diasAntes"/> dias a partir de hoje.
    /// Ordenado por data_vencimento ASC para priorizar os mais urgentes.
    /// </summary>
    public Task<IReadOnlyList<Prazo>> GetVencimentoProximoAsync(
        int diasAntes, CancellationToken ct = default)
    {
        var hoje   = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoje.AddDays(diasAntes);
        return _client.GetListAsync<Prazo>(TableName,
            $"status=eq.Pendente" +
            $"&data_vencimento=gte.{hoje:yyyy-MM-dd}" +
            $"&data_vencimento=lte.{limite:yyyy-MM-dd}" +
            $"&order=data_vencimento.asc",
            Token, ct);
    }

    /// <summary>
    /// GET todos os prazos com status=Pendente, ordenados por vencimento.
    /// </summary>
    public Task<IReadOnlyList<Prazo>> GetPendentesAsync(CancellationToken ct = default) =>
        _client.GetListAsync<Prazo>(TableName,
            "status=eq.Pendente&order=data_vencimento.asc",
            Token, ct);

    /// <summary>
    /// GET prazos com prioridade Fatal e status Pendente.
    /// </summary>
    public Task<IReadOnlyList<Prazo>> GetFataisAsync(CancellationToken ct = default) =>
        _client.GetListAsync<Prazo>(TableName,
            "status=eq.Pendente&prioridade=eq.Fatal&order=data_vencimento.asc",
            Token, ct);

    /// <summary>
    /// GET os próximos <paramref name="count"/> prazos Pendentes a partir de hoje.
    /// Usado pelo card "Próximos Prazos" do Dashboard.
    /// </summary>
    public Task<IReadOnlyList<Prazo>> GetProximosAsync(
        int count, CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return _client.GetListAsync<Prazo>(TableName,
            $"status=eq.Pendente" +
            $"&data_vencimento=gte.{hoje:yyyy-MM-dd}" +
            $"&order=data_vencimento.asc" +
            $"&limit={count}",
            Token, ct);
    }
}
