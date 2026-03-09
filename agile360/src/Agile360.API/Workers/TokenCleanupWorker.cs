using Agile360.Domain.Interfaces;

namespace Agile360.API.Workers;

/// <summary>
/// Background service that wakes up daily at 03:00 (UTC) to hard-delete
/// expired refresh token sessions from the database.
///
/// IServiceScopeFactory is used because the worker is registered as a
/// Singleton, while IRefreshTokenRepository is Scoped.
/// </summary>
public sealed class TokenCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupWorker> _logger;

    // Target hour (UTC) at which the cleanup runs every day
    private const int RunAtHourUtc = 3;

    public TokenCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TokenCleanupWorker iniciado. Limpeza agendada diariamente às {Hour:D2}:00 UTC.", RunAtHourUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogDebug("Próxima limpeza de tokens em {Delay:hh\\:mm\\:ss}.", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunCleanupAsync(stoppingToken);
        }

        _logger.LogInformation("TokenCleanupWorker encerrado.");
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        _logger.LogInformation("Iniciando limpeza de tokens...");
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var removed = await repo.DeleteExpiredAsync(ct);
            _logger.LogInformation("Limpeza concluída: {Count} tokens removidos.", removed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Erro durante a limpeza de tokens expirados.");
        }
    }

    /// <summary>
    /// Calculates the delay from now until the next 03:00 UTC.
    /// If it is already past 03:00 today, schedules for 03:00 tomorrow.
    /// </summary>
    private static TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTimeOffset.UtcNow;
        var nextRun = new DateTimeOffset(now.Year, now.Month, now.Day, RunAtHourUtc, 0, 0, TimeSpan.Zero);
        if (now >= nextRun)
            nextRun = nextRun.AddDays(1);
        return nextRun - now;
    }
}
