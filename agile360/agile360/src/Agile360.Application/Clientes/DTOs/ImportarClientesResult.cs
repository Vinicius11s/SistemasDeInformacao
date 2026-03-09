namespace Agile360.Application.Clientes.DTOs;

/// <summary>Retorno do endpoint POST /api/clientes/importar</summary>
public record ImportarClientesResult(
    int Total,
    int Sucesso,
    int Falhas,
    IReadOnlyList<ImportarClienteErro> Erros
);

public record ImportarClienteErro(
    int    Linha,
    string NomeCompleto,
    string Motivo
);
