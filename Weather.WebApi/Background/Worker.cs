using Microsoft.EntityFrameworkCore;

using Npgsql;

using Weather.WebApi.Data;

namespace Weather.WebApi.Background;

public sealed class Worker(IServiceScopeFactory scopeFactory) : BackgroundService, IDisposable
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1)); 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();
            var connection = (NpgsqlConnection)context.Database.GetDbConnection();
            
            await connection.OpenAsync(stoppingToken);

            var sql = @"
INSERT INTO ""WeatherEntries"" (""Timestamp"", ""Temperature"", ""Humidity"", ""Pressure"")
VALUES (@p1, @p2, @p3, @p4)";

            var insertCommand = new NpgsqlCommand(sql, connection)
            {
                Parameters =
                {
                    new() { ParameterName = "@p1", Value = DateTimeOffset.UtcNow },
                    new() { ParameterName = "@p2", Value = Random.Shared.Next(15, 16) + Random.Shared.NextDouble() },
                    new() { ParameterName = "@p3", Value = 12 },
                    new() { ParameterName = "@p4", Value = 15 },
                }
            };

            await insertCommand.ExecuteNonQueryAsync(stoppingToken);
        }
    }

    public override void Dispose()
    {
        _timer.Dispose();

        base.Dispose();
    }
}
