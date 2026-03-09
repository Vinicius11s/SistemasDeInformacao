using Agile360.Application.Clientes.DTOs;
using Agile360.Application.Clientes.Services;
using Agile360.API.Models;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClienteController(
    IClienteRepository repo,
    IClienteBulkImportService bulkImportService) : ControllerBase
{
    // ─── GET /api/clientes ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await repo.GetAllAsync(ct);
        return Ok(lista.Select(ToResponse));
    }

    // ─── GET /api/clientes/{id} ───────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(id, ct);
        return c is null ? NotFound() : Ok(ToResponse(c));
    }

    // ─── POST /api/clientes ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest req, CancellationToken ct)
    {
        var entity = FromCriar(req);
        var criado = await repo.AddAsync(entity, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ToResponse(criado));
    }

    // ─── PUT /api/clientes/{id} ───────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarClienteRequest req, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        AplicarAtualizacao(existente, req);
        await repo.UpdateAsync(existente, ct);
        return NoContent();
    }

    // ─── DELETE /api/clientes/{id} ────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var existente = await repo.GetByIdAsync(id, ct);
        if (existente is null) return NotFound();

        await repo.RemoveAsync(existente, ct);
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  IMPORTAÇÃO EM MASSA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// GET /api/clientes/template
    /// Retorna um arquivo .xlsx com as colunas corretas e aba de instruções.
    /// O usuário baixa, preenche e faz upload em /importar.
    /// </summary>
    [HttpGet("template")]
    public IActionResult BaixarTemplate()
    {
        using var wb = new XLWorkbook();

        // ── Aba principal ──────────────────────────────────────────────────────
        var ws = wb.Worksheets.Add("Clientes");

        var colunas = new[]
        {
            "nome_completo*", "cpf", "rg", "orgao_expedidor",
            "data_nascimento (dd/mm/aaaa)", "estado_civil", "profissao",
            "telefone", "numero_conta", "pix",
            "cep", "endereco", "numero", "bairro", "complemento",
            "cidade", "estado (2 letras)"
        };

        for (int i = 0; i < colunas.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = colunas[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            cell.Style.Font.FontColor       = XLColor.White;
        }

        // Linha de exemplo
        var ex = new[]
        {
            "Maria da Silva", "123.456.789-00", "12.345.678-9", "SSP/SP",
            "15/05/1985", "Casada", "Professora",
            "(11) 98765-4321", "0001 / 00012345-6", "chave@pix.com",
            "01310-100", "Avenida Paulista", "1000", "Bela Vista", "Ap. 42",
            "São Paulo", "SP"
        };
        for (int i = 0; i < ex.Length; i++)
            ws.Cell(2, i + 1).Value = ex[i];

        ws.Columns().AdjustToContents();

        // ── Aba de instruções ──────────────────────────────────────────────────
        var wsInst = wb.Worksheets.Add("Instruções");
        var instrucoes = new[]
        {
            "INSTRUÇÕES DE PREENCHIMENTO",
            "",
            "1. Preencha a aba 'Clientes' a partir da linha 2.",
            "2. Não remova nem renomeie as colunas do cabeçalho.",
            "3. A coluna nome_completo* é obrigatória.",
            "4. data_nascimento: use o formato dd/mm/aaaa  (ex.: 15/05/1985).",
            "5. estado: use apenas 2 letras maiúsculas (SP, RJ, MG, etc.).",
            "6. Campos vazios são aceitos para as colunas opcionais.",
            "7. Após preencher, salve como .xlsx e envie em 'Cadastro em massa'.",
        };
        for (int i = 0; i < instrucoes.Length; i++)
        {
            wsInst.Cell(i + 1, 1).Value = instrucoes[i];
            if (i == 0) wsInst.Cell(i + 1, 1).Style.Font.Bold = true;
        }
        wsInst.Column(1).AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "modelo_clientes.xlsx");
    }

    /// <summary>
    /// POST /api/clientes/importar   (multipart/form-data, campo: planilha)
    /// Responsabilidade do Controller: APENAS parse do Excel → extração de linhas.
    /// Toda a lógica de negócio (validação, deduplicação, batch insert) fica em
    /// IClienteBulkImportService — testável de forma independente.
    /// </summary>
    [HttpPost("importar")]
    [RequestSizeLimit(10 * 1024 * 1024)] // AC-2: máx 10 MB
    public async Task<IActionResult> Importar(
        IFormFile planilha, CancellationToken ct)
    {
        // ── Validações de infraestrutura (arquivo) ────────────────────────────
        if (planilha is null || planilha.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Nenhum arquivo enviado."));

        if (!planilha.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object>.Fail("O arquivo deve ser .xlsx."));

        using var stream = planilha.OpenReadStream();
        XLWorkbook wb;
        try { wb = new XLWorkbook(stream); }
        catch { return BadRequest(ApiResponse<object>.Fail("Arquivo .xlsx inválido ou corrompido.")); }

        using (wb)
        {
            if (!wb.Worksheets.Contains("Clientes"))
                return BadRequest(ApiResponse<object>.Fail(
                    "A aba 'Clientes' não foi encontrada. Use o modelo disponível em Passo 1."));

            var ws      = wb.Worksheet("Clientes");
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            // AC-2: limite de 500 linhas de dados (linha 1 = cabeçalho)
            if (lastRow - 1 > ClienteBulkImportService.MaxLinhas)
                return BadRequest(ApiResponse<object>.Fail(
                    $"A planilha excede o limite de {ClienteBulkImportService.MaxLinhas} linhas. Divida em partes menores."));

            // ── Extração de linhas (responsabilidade do Controller/API layer) ─
            var linhas = new List<ImportarClienteRow>();
            for (int row = 2; row <= lastRow; row++)
            {
                var nome = ws.Cell(row, 1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(nome)) continue; // linha vazia — ignora

                linhas.Add(new ImportarClienteRow(
                    Linha:              row,
                    NomeCompleto:       nome,
                    Cpf:                NullIfEmpty(ws.Cell(row,  2).GetString()),
                    Rg:                 NullIfEmpty(ws.Cell(row,  3).GetString()),
                    OrgaoExpedidor:     NullIfEmpty(ws.Cell(row,  4).GetString()),
                    DataNascimentoRaw:  ExtractDateString(ws.Cell(row, 5)),
                    EstadoCivil:        NullIfEmpty(ws.Cell(row,  6).GetString()),
                    Profissao:          NullIfEmpty(ws.Cell(row,  7).GetString()),
                    Telefone:           NullIfEmpty(ws.Cell(row,  8).GetString()),
                    NumeroConta:        NullIfEmpty(ws.Cell(row,  9).GetString()),
                    Pix:                NullIfEmpty(ws.Cell(row, 10).GetString()),
                    Cep:                NullIfEmpty(ws.Cell(row, 11).GetString()),
                    Endereco:           NullIfEmpty(ws.Cell(row, 12).GetString()),
                    Numero:             NullIfEmpty(ws.Cell(row, 13).GetString()),
                    Bairro:             NullIfEmpty(ws.Cell(row, 14).GetString()),
                    Complemento:        NullIfEmpty(ws.Cell(row, 15).GetString()),
                    Cidade:             NullIfEmpty(ws.Cell(row, 16).GetString()),
                    Estado:             NullIfEmpty(ws.Cell(row, 17).GetString())
                ));
            }

            if (linhas.Count == 0)
                return BadRequest(ApiResponse<object>.Fail(
                    "A planilha não contém linhas de dados. Preencha a partir da linha 2."));

            // ── Delega toda a lógica de negócio ao service ────────────────────
            var resultado = await bulkImportService.ImportarAsync(linhas, ct);
            return Ok(resultado);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>
    /// Lê uma célula de data do Excel de forma segura:
    /// - Se o tipo da célula for DateTime (célula formatada como Data no Excel),
    ///   converte para string "dd/MM/yyyy".
    /// - Se for texto, retorna a string como está (para o Service validar).
    /// - Célula vazia retorna null.
    /// </summary>
    private static string? ExtractDateString(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;

        // Célula formatada como Data no Excel → ClosedXML armazena como DateTime
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("dd/MM/yyyy");

        // Célula com texto digitado (ex: "15/05/1985")
        var s = cell.GetString().Trim();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static Cliente FromCriar(CriarClienteRequest r) => new()
    {
        TipoCliente    = r.TipoCliente,
        NomeCompleto   = r.NomeCompleto,
        Cpf            = r.Cpf,
        Rg             = r.Rg,
        OrgaoExpedidor = r.OrgaoExpedidor,
        DataNascimento = r.DataNascimento,
        EstadoCivil    = r.EstadoCivil,
        Profissao      = r.Profissao,
        Telefone       = r.Telefone,
        NumeroConta    = r.NumeroConta,
        Pix            = r.Pix,
        Cep            = r.Cep,
        Endereco       = r.Endereco,
        Numero         = r.Numero,
        Bairro         = r.Bairro,
        Complemento    = r.Complemento,
        Cidade         = r.Cidade,
        Estado         = r.Estado,
    };

    private static void AplicarAtualizacao(Cliente c, AtualizarClienteRequest r)
    {
        c.TipoCliente    = r.TipoCliente;
        c.NomeCompleto   = r.NomeCompleto;
        c.Cpf            = r.Cpf;
        c.Rg             = r.Rg;
        c.OrgaoExpedidor = r.OrgaoExpedidor;
        c.DataNascimento = r.DataNascimento;
        c.EstadoCivil    = r.EstadoCivil;
        c.Profissao      = r.Profissao;
        c.Telefone       = r.Telefone;
        c.NumeroConta    = r.NumeroConta;
        c.Pix            = r.Pix;
        c.Cep            = r.Cep;
        c.Endereco       = r.Endereco;
        c.Numero         = r.Numero;
        c.Bairro         = r.Bairro;
        c.Complemento    = r.Complemento;
        c.Cidade         = r.Cidade;
        c.Estado         = r.Estado;
    }

    private static ClienteResponse ToResponse(Cliente c) => new(
        c.Id, c.IdAdvogado,
        c.TipoCliente, c.NomeCompleto, c.Cpf, c.Rg, c.OrgaoExpedidor,
        c.DataNascimento, c.EstadoCivil, c.Profissao,
        c.Telefone, c.NumeroConta, c.Pix,
        c.Cep, c.Endereco, c.Numero, c.Bairro, c.Complemento,
        c.Cidade, c.Estado, c.DataCadastro);
}
