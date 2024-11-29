
namespace Weather.WebApi.Data;

public class WeatherEntry
{
    public required DateTimeOffset Timestamp { get; init; }

    public required double Temperature { get; init; }

    public required double Humidity { get; init; }

    public required double Pressure { get; init; }
}
