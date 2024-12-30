namespace Weather.WebApi.Api.Models;

public class WeatherEntryDto
{
    public required DateTimeOffset Timestamp { get; init; }

    public required double Temperature { get; init; }

    public required double Humidity { get; init; }

    public required double Pressure { get; init; }
}
