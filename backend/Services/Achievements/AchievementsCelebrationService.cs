using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Services.Achievements;

public class AchievementsCelebrationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AchievementsCelebrationService> _logger;
    private static readonly TimeSpan ScheduledTimeUtc = new(0, 5, 0);

    public AchievementsCelebrationService(IServiceScopeFactory scopeFactory, ILogger<AchievementsCelebrationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun(DateTime.UtcNow, ScheduledTimeUtc);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<AchievementsService>();
                await service.EnsureDailyCelebrationsAsync(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate daily achievements celebrations.");
            }
        }
    }

    private static TimeSpan GetDelayUntilNextRun(DateTime nowUtc, TimeSpan scheduledTimeUtc)
    {
        var nextRun = nowUtc.Date.Add(scheduledTimeUtc);
        if (nextRun <= nowUtc)
        {
            nextRun = nextRun.AddDays(1);
        }

        var delay = nextRun - nowUtc;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }
}