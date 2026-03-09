namespace Agile360.Application.Interfaces;

/// <summary>
/// Serviço de Códigos de Recuperação de Emergência para MFA.
///
/// Contrato de segurança:
///   - O texto limpo dos códigos é retornado APENAS na geração (uma única vez).
///   - Somente o hash BCrypt é persistido no banco — nunca o plaintext.
///   - Burn-after-use: código validado com sucesso tem IsUsed = true e nunca é aceito novamente.
///   - A operação de consumo é atomicamente protegida contra race condition.
/// </summary>
public interface IRecoveryCodeService
{
    /// <summary>
    /// Gera 10 novos códigos de recuperação, invalida todos os anteriores do advogado
    /// e persiste apenas os hashes BCrypt (cost 12).
    ///
    /// ÚNICA oportunidade de exposição do plaintext — o chamador deve repassá-los
    /// imediatamente ao frontend e nunca armazená-los.
    /// </summary>
    /// <param name="advogadoId">ID do advogado para quem os códigos são gerados.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Lista com os 10 códigos em formato XXXX-XXXX (plaintext, exibição única).</returns>
    Task<IReadOnlyList<string>> GenerateCodesAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>
    /// Valida um código de recuperação e, se válido, marca como usado (burn-after-use).
    ///
    /// A operação é atômica via transação com UPDATE WHERE is_used = false.
    /// Duas requisições simultâneas com o mesmo código não são aceitas simultaneamente.
    /// </summary>
    /// <param name="advogadoId">ID do advogado.</param>
    /// <param name="code">Código de recuperação em formato XXXX-XXXX fornecido pelo usuário.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>true se o código era válido e foi consumido; false caso contrário.</returns>
    Task<bool> ValidateAndConsumeAsync(Guid advogadoId, string code, CancellationToken ct = default);

    /// <summary>
    /// Retorna a quantidade de códigos de recuperação ainda não utilizados pelo advogado.
    /// </summary>
    /// <param name="advogadoId">ID do advogado.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Número de códigos disponíveis (0–10).</returns>
    Task<int> GetRemainingCountAsync(Guid advogadoId, CancellationToken ct = default);

    /// <summary>
    /// Remove todos os códigos de recuperação do advogado (hard delete).
    /// Chamado ao desativar o MFA ou ao gerar novos códigos (invalidação prévia).
    /// </summary>
    /// <param name="advogadoId">ID do advogado.</param>
    /// <param name="ct">Token de cancelamento.</param>
    Task DeleteAllAsync(Guid advogadoId, CancellationToken ct = default);
}
