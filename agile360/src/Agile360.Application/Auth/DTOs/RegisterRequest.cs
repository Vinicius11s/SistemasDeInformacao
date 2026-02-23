namespace Agile360.Application.Auth.DTOs;

public record RegisterRequest(string Nome, string Email, string Password, string OAB, string? Telefone);
