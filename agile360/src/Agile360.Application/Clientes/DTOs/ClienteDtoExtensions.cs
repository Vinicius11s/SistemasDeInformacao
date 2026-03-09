using Agile360.Shared;

namespace Agile360.Application.Clientes.DTOs;

public static class ClienteDtoExtensions
{
    public static CreateClienteRequest Sanitize(this CreateClienteRequest r) => r with
    {
        CPF               = DocumentSanitizer.Sanitize(r.CPF),
        CNPJ              = DocumentSanitizer.Sanitize(r.CNPJ),
        RG                = DocumentSanitizer.Sanitize(r.RG),
        InscricaoEstadual = DocumentSanitizer.Sanitize(r.InscricaoEstadual),
        Telefone          = DocumentSanitizer.Sanitize(r.Telefone),
    };

    public static UpdateClienteRequest Sanitize(this UpdateClienteRequest r) => r with
    {
        CPF               = DocumentSanitizer.Sanitize(r.CPF),
        CNPJ              = DocumentSanitizer.Sanitize(r.CNPJ),
        RG                = DocumentSanitizer.Sanitize(r.RG),
        InscricaoEstadual = DocumentSanitizer.Sanitize(r.InscricaoEstadual),
        Telefone          = DocumentSanitizer.Sanitize(r.Telefone),
    };
}
