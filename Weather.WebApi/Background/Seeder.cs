using Microsoft.EntityFrameworkCore;

using Weather.WebApi.Data;

namespace Weather.WebApi.Background;

public class Seeder(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var snapshots = scope.ServiceProvider.GetRequiredService<ISnapshots>();
        var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();
        var firstSnapshot = await snapshots.GetAsync(cancellationToken);

        if (await context.WeatherSnapshots.Where(s => s.Timestamp == firstSnapshot.Timestamp).AnyAsync(cancellationToken))
        {
            return;
        }

        await context.Database.ExecuteSqlAsync(
            $"""
                INSERT INTO "WeatherSnapshots" ("Timestamp", "Temperature", "Humidity", "Pressure")
                VALUES ({firstSnapshot.Timestamp}, {firstSnapshot.Temperature}, {firstSnapshot.Humidity}, {firstSnapshot.Pressure})
            """,
            cancellationToken);

        foreach (var i in Enumerable.Range(1, 8738))
        {
            var snapshot = await snapshots.GetAsync(cancellationToken);

            await context.Database.ExecuteSqlAsync(
                $"""
                    INSERT INTO "WeatherSnapshots" ("Timestamp", "Temperature", "Humidity", "Pressure")
                    VALUES ({snapshot.Timestamp}, {snapshot.Temperature}, {snapshot.Humidity}, {snapshot.Pressure})
                """,
                cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
