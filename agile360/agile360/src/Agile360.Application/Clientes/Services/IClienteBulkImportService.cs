using Agile360.Application.Clientes.DTOs;

namespace Agile360.Application.Clientes.Services;

/// <summary>
/// Orquestra a importação em massa de clientes a partir de linhas
/// já extraídas do Excel.
/// Responsabilidades:
///   1. Validar formato de CPF
///   2. Detectar duplicados (CPF já existente para o advogado logado)
///   3. Persistir os registros válidos em lote (batch insert)
///   4. Retornar resumo com sucesso e erros por linha
/// </summary>
public interface IClienteBulkImportService
{
    Task<ImportarClientesResult> ImportarAsync(
        IReadOnlyList<ImportarClienteRow> linhas,
        CancellationToken ct = default);
}
