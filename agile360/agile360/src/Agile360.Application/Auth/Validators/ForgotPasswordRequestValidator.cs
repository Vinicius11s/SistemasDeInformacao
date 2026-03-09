using Agile360.Application.Auth.DTOs;
using FluentValidation;

namespace Agile360.Application.Auth.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");
    }
}
