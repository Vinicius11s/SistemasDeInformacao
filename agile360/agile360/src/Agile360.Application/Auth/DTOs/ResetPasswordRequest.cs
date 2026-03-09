namespace Agile360.Application.Auth.DTOs;

public record ResetPasswordRequest(string Token, string NewPassword);
