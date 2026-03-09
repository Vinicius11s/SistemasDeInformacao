using Agile360.Application.Auth.DTOs;
using FluentValidation;

namespace Agile360.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória");
    }
}
