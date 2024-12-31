using Microsoft.EntityFrameworkCore;

using Weather.WebApi.Data;

namespace Weather.WebApi.Background;

public sealed class PollingWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1)); 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var snapshots = scope.ServiceProvider.GetRequiredService<ISnapshots>();
            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();
            var snapshot = await snapshots.GetAsync(stoppingToken);

            await context.Database.ExecuteSqlAsync(
                $"""
                    INSERT INTO "WeatherSnapshots" ("Timestamp", "Temperature", "Humidity", "Pressure")
                    VALUES ({snapshot.Timestamp}, {snapshot.Temperature}, {snapshot.Humidity}, {snapshot.Pressure})
                """,
                stoppingToken);
        }
    }

    public override void Dispose()
    {
        _timer.Dispose();

        base.Dispose();
    }
}
