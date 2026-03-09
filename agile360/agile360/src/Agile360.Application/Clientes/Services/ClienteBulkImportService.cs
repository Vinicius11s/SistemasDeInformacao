using System.Globalization;
using System.Text.RegularExpressions;
using Agile360.Application.Clientes.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;

namespace Agile360.Application.Clientes.Services;

/// <summary>
/// Implementa o fluxo de importação em massa de clientes:
///   Validação → Deduplicação → Batch Insert (1 chamada HTTP ao Supabase).
/// </summary>
public sealed partial class ClienteBulkImportService(
    IClienteRepository repo) : IClienteBulkImportService
{
    public const int MaxLinhas = 500;

    /// <summary>
    /// Aceita CPF com ou sem formatação.
    /// Exemplos válidos: "123.456.789-00"  |  "12345678900"
    /// </summary>
    [GeneratedRegex(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$")]
    private static partial Regex CpfRegex();


    public async Task<ImportarClientesResult> ImportarAsync(
        IReadOnlyList<ImportarClienteRow> linhas,
        CancellationToken ct = default)
    {
        var erros      = new List<ImportarClienteErro>();
        var paraInserir = new List<Cliente>();

        // ── 1. Coleta CPFs existentes na base para deduplicação eficiente ─────
        //    Evita N+1: busca apenas os CPFs que aparecem na planilha.
        var cpfsNaPlanilha = linhas
            .Where(l => !string.IsNullOrWhiteSpace(l.Cpf))
            .Select(l => NormalizarCpf(l.Cpf!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cpfsJaCadastrados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cpf in cpfsNaPlanilha)
        {
            var existente = await repo.GetByCpfAsync(cpf, ct);
            if (existente is not null)
                cpfsJaCadastrados.Add(cpf);
        }

        // ── 2. Valida e classifica cada linha ─────────────────────────────────
        foreach (var linha in linhas)
        {
            // AC-2 / AC-3: nome obrigatório (linhas sem nome são ignoradas antes de chegar aqui)
            if (string.IsNullOrWhiteSpace(linha.NomeCompleto))
                continue;

            // AC-4: validação de formato CPF
            if (!string.IsNullOrWhiteSpace(linha.Cpf))
            {
                var cpfNorm = NormalizarCpf(linha.Cpf);
                if (!CpfRegex().IsMatch(cpfNorm))
                {
                    erros.Add(new(linha.Linha, linha.NomeCompleto,
                        $"CPF inválido: '{linha.Cpf}'. Informe 11 dígitos (com ou sem pontuação)."));
                    continue;
                }

                // AC-5: deduplicação por CPF
                if (cpfsJaCadastrados.Contains(cpfNorm))
                {
                    erros.Add(new(linha.Linha, linha.NomeCompleto,
                        $"CPF '{linha.Cpf}' já está cadastrado para este advogado."));
                    continue;
                }

                // Marca o CPF como "será inserido" para evitar duplicatas internas na própria planilha
                cpfsJaCadastrados.Add(cpfNorm);
            }

            // AC-8: validação de data de nascimento
            DateOnly? dataNasc = null;
            if (!string.IsNullOrWhiteSpace(linha.DataNascimentoRaw))
            {
                // Aceita: 15/05/1985 · 15/5/1985 · 5/5/1985 (digitado como texto)
                // Células formatadas como Data no Excel já chegam em dd/MM/yyyy via ExtractDateString
                string[] formatos = ["dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy",
                                     "yyyy-MM-dd", "MM/dd/yyyy"];
                if (!DateOnly.TryParseExact(linha.DataNascimentoRaw, formatos,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dn))
                {
                    erros.Add(new(linha.Linha, linha.NomeCompleto,
                        $"data_nascimento inválida: '{linha.DataNascimentoRaw}'. " +
                        "Use o formato dd/mm/aaaa (ex: 15/05/1985) ou formate a célula como Data no Excel."));
                    continue;
                }
                dataNasc = dn;
            }

            // AC-9: validação de estado
            var estado = linha.Estado?.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(estado) && estado.Length > 2)
            {
                erros.Add(new(linha.Linha, linha.NomeCompleto,
                    $"estado deve ter no máximo 2 letras (recebido: '{estado}')."));
                continue;
            }

            // Linha válida — mapeia para entidade (IdAdvogado será setado pelo repository)
            paraInserir.Add(new Cliente
            {
                NomeCompleto   = linha.NomeCompleto.Trim(),
                Cpf            = NullIfEmpty(linha.Cpf),
                Rg             = NullIfEmpty(linha.Rg),
                OrgaoExpedidor = NullIfEmpty(linha.OrgaoExpedidor),
                DataNascimento = dataNasc,
                EstadoCivil    = NullIfEmpty(linha.EstadoCivil),
                Profissao      = NullIfEmpty(linha.Profissao),
                Telefone       = NullIfEmpty(linha.Telefone),
                NumeroConta    = NullIfEmpty(linha.NumeroConta),
                Pix            = NullIfEmpty(linha.Pix),
                Cep            = NullIfEmpty(linha.Cep),
                Endereco       = NullIfEmpty(linha.Endereco),
                Numero         = NullIfEmpty(linha.Numero),
                Bairro         = NullIfEmpty(linha.Bairro),
                Complemento    = NullIfEmpty(linha.Complemento),
                Cidade         = NullIfEmpty(linha.Cidade),
                Estado         = string.IsNullOrWhiteSpace(estado) ? null : estado,
            });
        }

        // ── 3. Batch insert — 1 chamada HTTP ao Supabase para todos os válidos ─
        var inseridos = paraInserir.Count > 0
            ? await repo.AddRangeAsync(paraInserir, ct)
            : Array.Empty<Cliente>();

        return new ImportarClientesResult(
            Total:   linhas.Count,
            Sucesso: inseridos.Count,
            Falhas:  erros.Count,
            Erros:   erros);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>Remove pontuação do CPF para normalização antes da comparação.</summary>
    private static string NormalizarCpf(string cpf) =>
        cpf.Replace(".", "").Replace("-", "").Trim();

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
