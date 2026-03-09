using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;

namespace Agile360.Infrastructure.Repositories;

/// <summary>
/// Repositório genérico via Supabase PostgREST.
/// Tabelas no singular (cliente, processo, compromisso).
/// FKs: id_advogado (tenant), id_cliente, id_processo (snake_case via SnakeCaseLower).
/// RLS do Supabase filtra automaticamente pelo Bearer token.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly SupabaseDataClient _client;
    protected readonly ICurrentUserService _currentUser;

    private static readonly Dictionary<Type, string> TableNames = new()
    {
        [typeof(Cliente)]    = "cliente",
        [typeof(Processo)]   = "processo",
        [typeof(Compromisso)]= "compromisso",
        [typeof(Prazo)]      = "prazo",
    };

    protected virtual string TableName =>
        TableNames.TryGetValue(typeof(T), out var n) ? n : typeof(T).Name.ToLowerInvariant();

    protected string Token =>
        _currentUser.AccessToken
        ?? throw new InvalidOperationException("Usuário não autenticado: AccessToken ausente.");

    public Repository(SupabaseDataClient client, ICurrentUserService currentUser)
    {
        _client      = client;
        _currentUser = currentUser;
    }

    // GET /rest/v1/{table}?id=eq.{id}&limit=1
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _client.GetSingleAsync<T>(TableName, $"id=eq.{id}", Token, ct);

    // GET /rest/v1/{table}
    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        _client.GetListAsync<T>(TableName, string.Empty, Token, ct);

    // POST /rest/v1/{table}    Prefer: return=representation
    /// <summary>
    /// Virtual para permitir overrides nos repositórios concretos.
    /// Atenção: os campos NOT NULL sem DEFAULT (ex.: criado_em) devem ser
    /// setados pelo repositório filho antes de chamar base.AddAsync,
    /// caso contrário o PostgREST devolve 400 com o código 23502.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        entity.IdAdvogado = _currentUser.AdvogadoId;
        return await _client.InsertAsync<T>(TableName, entity, Token, ct) ?? entity;
    }

    // PATCH /rest/v1/{table}?id=eq.{id}
    public Task UpdateAsync(T entity, CancellationToken ct = default) =>
        _client.PatchAsync(TableName, $"id=eq.{entity.Id}", entity, Token, ct);

    // DELETE /rest/v1/{table}?id=eq.{id}
    public Task RemoveAsync(T entity, CancellationToken ct = default) =>
        _client.DeleteAsync(TableName, $"id=eq.{entity.Id}", Token, ct);
}
