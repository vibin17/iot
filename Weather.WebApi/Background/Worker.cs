using Microsoft.EntityFrameworkCore;

using Npgsql;

using System.Globalization;

using Weather.WebApi.Data;

namespace Weather.WebApi.Background;

public sealed class Worker(IServiceScopeFactory scopeFactory) : BackgroundService, IDisposable
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1)); 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var lines = await File.ReadAllLinesAsync("pollution.csv", stoppingToken);
        var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        await connection.OpenAsync(stoppingToken);

        foreach (var line in lines.Skip(1))
        {
            var values = line.Split(",");
            var timestamp = DateTimeOffset.Parse(values[0]).AddYears(13);
            var td = double.Parse(values[2], CultureInfo.InvariantCulture);
            var temperature = double.Parse(values[3], CultureInfo.InvariantCulture);
            var pressure = double.Parse(values[4], CultureInfo.InvariantCulture);
            var humidity = GetHumidity(td, temperature);

            const string Sql = 
@"INSERT INTO ""WeatherEntries"" (""Timestamp"", ""Temperature"", ""Humidity"", ""Pressure"")
VALUES (@p1, @p2, @p3, @p4)";

            using var insertCommand = new NpgsqlCommand(Sql, connection)
            {
                Parameters =
                {
                    new() { ParameterName = "@p1", Value = timestamp },
                    new() { ParameterName = "@p2", Value = temperature },
                    new() { ParameterName = "@p3", Value = humidity },
                    new() { ParameterName = "@p4", Value = pressure },
                }
            };

            await insertCommand.ExecuteNonQueryAsync(stoppingToken);
        }
    }

    private double GetHumidity(double td, double t)
    {
        const double A = 6.116441, M = 7.591386, Tn = 240.7263;

        return 100 * Math.Pow(10, M * (td / (td + Tn) - t / (t + Tn)));
    }

    public override void Dispose()
    {
        _timer.Dispose();

        base.Dispose();
    }
}
