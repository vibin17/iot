
using System.Globalization;

namespace Weather.WebApi.Data;

public class Snapshots : ISnapshots, IDisposable
{
    private int _counter = 0; // skip table header
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyCollection<string>? _lines;

    public async Task<WeatherSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        var lines = await GetLinesAsync(cancellationToken);

        Interlocked.Increment(ref _counter);

        var line = lines.ElementAt(_counter);
        var values = line.Split(",");
        var timestamp = DateTimeOffset.Parse(values[0]).AddYears(14).ToUniversalTime();
        var td = double.Parse(values[2], CultureInfo.InvariantCulture);
        var temperature = double.Parse(values[3], CultureInfo.InvariantCulture);
        var pressure = double.Parse(values[4], CultureInfo.InvariantCulture);
        var humidity = GetHumidity(td, temperature);

        return new()
        {
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            Pressure = pressure
        };
    }

    private double GetHumidity(double td, double t)
    {
        const double A = 6.116441, M = 7.591386, Tn = 240.7263;

        return 100 * Math.Pow(10, M * (td / (td + Tn) - t / (t + Tn)));
    }

    private async Task<IReadOnlyCollection<string>> GetLinesAsync(CancellationToken cancellationToken)
    {
        if (_lines is not null)
        {
            return _lines;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_lines is not null)
            {
                return _lines;
            }

            var lines = await File.ReadAllLinesAsync("pollution.csv", cancellationToken);

            _lines = lines;

            return _lines;

        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
