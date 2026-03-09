using Agile360.Application.Auth.DTOs;
using FluentValidation;

namespace Agile360.Application.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres");
        RuleFor(x => x.OAB)
            .NotEmpty().WithMessage("OAB é obrigatória")
            .MaximumLength(20)
            .Matches(@"^OAB/[A-Z]{2}\s*\d+$").WithMessage("OAB deve estar no formato OAB/UF número (ex: OAB/SP 123456)");
        RuleFor(x => x.Telefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Telefone));
    }
}
