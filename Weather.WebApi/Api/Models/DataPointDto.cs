namespace Weather.WebApi.Api.Models;

public class DataPointDto
{
    public required DateTimeOffset Timestamp { get; init; }

    public required double Value { get; init; }
}
