using Agile360.Application.Clientes.DTOs;
using FluentValidation;

namespace Agile360.Application.Clientes.Validators;

public class CreateClienteRequestValidator : AbstractValidator<CreateClienteRequest>
{
    public CreateClienteRequestValidator()
    {
        RuleFor(x => x.TipoCliente)
            .NotEmpty().WithMessage("TipoCliente é obrigatório.")
            .Must(t => t is "Pessoa Física" or "Pessoa Jurídica")
            .WithMessage("TipoCliente deve ser 'Pessoa Física' ou 'Pessoa Jurídica'.");

        When(x => x.TipoCliente == "Pessoa Física", () =>
        {
            RuleFor(x => x.NomeCompleto)
                .NotEmpty().WithMessage("Nome é obrigatório para Pessoa Física.")
                .MaximumLength(300);

            RuleFor(x => x.CPF)
                .Must(cpf => string.IsNullOrEmpty(cpf) || BeValidCpf(cpf))
                .WithMessage("CPF inválido. Informe os 11 dígitos corretos.");
        });

        When(x => x.TipoCliente == "Pessoa Jurídica", () =>
        {
            RuleFor(x => x.RazaoSocial)
                .NotEmpty().WithMessage("Razão Social é obrigatória para Pessoa Jurídica.")
                .MaximumLength(300);

            RuleFor(x => x.CNPJ)
                .NotEmpty().WithMessage("CNPJ é obrigatório para Pessoa Jurídica.")
                .Must(cnpj => string.IsNullOrEmpty(cnpj) || BeValidCnpj(cnpj))
                .WithMessage("CNPJ inválido. Informe os 14 dígitos corretos.");
        });
    }

    private static bool BeValidCpf(string raw)
    {
        var digits = OnlyDigits(raw);
        if (digits.Length != 11 || digits.Distinct().Count() == 1) return false;

        int Sum(int count, int offset)
        {
            var s = 0;
            for (var i = 0; i < count; i++) s += (digits[i] - '0') * (offset - i);
            var rem = s % 11;
            return rem < 2 ? 0 : 11 - rem;
        }
        return Sum(9, 10) == digits[9] - '0' && Sum(10, 11) == digits[10] - '0';
    }

    private static bool BeValidCnpj(string raw)
    {
        var digits = OnlyDigits(raw);
        if (digits.Length != 14 || digits.Distinct().Count() == 1) return false;

        int Calc(string d, int[] weights)
        {
            var sum = weights.Select((w, i) => w * (d[i] - '0')).Sum();
            var rem = sum % 11;
            return rem < 2 ? 0 : 11 - rem;
        }

        return Calc(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == digits[12] - '0' &&
               Calc(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == digits[13] - '0';
    }

    private static string OnlyDigits(string s) =>
        new(s.Where(char.IsDigit).ToArray());
}
