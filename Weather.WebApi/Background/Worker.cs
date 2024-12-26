using Weather.WebApi.Data;

namespace Weather.WebApi.Background;

public class Worker(IServiceScopeFactory scopeFactory) : BackgroundService, IDisposable
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(30)); 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            context.WeatherEntries.Add(new()
            {
                Timestamp = DateTime.UtcNow,
                Temperature = Random.Shared.Next(15, 16) + Random.Shared.NextDouble(),
                Humidity = 12,
                Pressure = 15
            });

            context.WeatherEntries.

            await context.SaveChangesAsync(stoppingToken);
        }
    }

    public override void Dispose()
    {
        _timer.Dispose();

        base.Dispose();
    }
}
