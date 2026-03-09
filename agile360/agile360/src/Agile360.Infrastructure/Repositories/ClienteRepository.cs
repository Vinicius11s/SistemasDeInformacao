using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure.Data;

namespace Agile360.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(SupabaseDataClient client, ICurrentUserService currentUser)
        : base(client, currentUser) { }

    // GET /rest/v1/cliente?cpf=eq.{cpf}&limit=1
    public Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken ct = default) =>
        _client.GetSingleAsync<Cliente>(TableName,
            $"cpf=eq.{Uri.EscapeDataString(cpf)}", Token, ct);

    // GET /rest/v1/cliente?or=(nome_completo.ilike.*t*,cpf.ilike.*t*,telefone.ilike.*t*)
    public Task<IReadOnlyList<Cliente>> SearchAsync(string termo, CancellationToken ct = default)
    {
        var t      = Uri.EscapeDataString($"*{termo}*");
        var filter = $"or=(nome_completo.ilike.{t},cpf.ilike.{t},telefone.ilike.{t})";
        return _client.GetListAsync<Cliente>(TableName, filter, Token, ct);
    }

    /// <summary>
    /// Batch Insert — 1 requisição HTTP ao Supabase PostgREST para N registros.
    /// POST /rest/v1/cliente  com body [ {...}, {...}, ... ]
    ///
    /// Usa <see cref="ClienteInsertDto"/> em vez de <see cref="Cliente"/> diretamente
    /// para evitar o erro PGRST102 do PostgREST ("All object keys must match"):
    ///   • <c>WhenWritingNull</c> no JsonOpts global omitiria campos nulos, fazendo
    ///     objetos de linhas parciais terem menos chaves que os de linhas completas.
    ///   • O DTO é serializado com <c>JsonOptsBatchInsert</c> (sem WhenWritingNull),
    ///     garantindo que todos os objetos do array tenham exatamente as mesmas chaves.
    ///   • <c>data_cadastro</c> é omitido do DTO → banco usa DEFAULT now().
    /// </summary>
    public async Task<IReadOnlyList<Cliente>> AddRangeAsync(
        IEnumerable<Cliente> clientes, CancellationToken ct = default)
    {
        var lista = clientes.ToList();
        if (lista.Count == 0) return [];

        // Projeta para ClienteInsertDto: garante todas as chaves presentes + exclui data_cadastro
        var dtos = lista.Select(c =>
        {
            if (c.Id == Guid.Empty) c.Id = Guid.NewGuid();
            return new ClienteInsertDto(
                Id:             c.Id,
                IdAdvogado:     _currentUser.AdvogadoId,
                TipoCliente:    c.TipoCliente,
                NomeCompleto:   c.NomeCompleto,
                Cpf:            c.Cpf,
                Rg:             c.Rg,
                OrgaoExpedidor: c.OrgaoExpedidor,
                DataNascimento: c.DataNascimento,
                EstadoCivil:    c.EstadoCivil,
                Profissao:      c.Profissao,
                Telefone:       c.Telefone,
                NumeroConta:    c.NumeroConta,
                Pix:            c.Pix,
                Cep:            c.Cep,
                Endereco:       c.Endereco,
                Numero:         c.Numero,
                Bairro:         c.Bairro,
                Complemento:    c.Complemento,
                Cidade:         c.Cidade,
                Estado:         c.Estado
            );
        }).ToList();

        // TIn = ClienteInsertDto (serializado sem WhenWritingNull → PGRST102 resolvido)
        // TOut = Cliente         (desserializado da resposta do Supabase)
        return await _client.InsertBatchAsync<ClienteInsertDto, Cliente>(TableName, dtos, Token, ct);
    }
}
