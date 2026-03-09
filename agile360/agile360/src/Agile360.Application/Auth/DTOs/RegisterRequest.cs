namespace Agile360.Application.Auth.DTOs;

public record RegisterRequest(
    string  Nome,
    string  Email,
    string  Password,
    string? OAB,              // numero_oab
    string? OabUf,            // oab_uf (max 2 chars — ex.: "SP")
    string? Telefone,         // telefone_contato
    string? NomeEscritorio);  // nome_escritorio
