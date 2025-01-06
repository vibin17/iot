using Refit;

using Weather.ClientApp.Models;

namespace Weather.ClientApp.Api;

public interface IWeatherApi
{
    [Get("/current")]
    Task<WeatherSnapshotDto> GetCurrentAsync(CancellationToken cancellationToken);

    [Get("/history/temperature")]
    Task<IReadOnlyCollection<DataPointDto>> GetTemperatureHistoryAsync([Query] DateTimeOffset from, [Query] DateTimeOffset to, CancellationToken cancellationToken);

    [Get("/history/humidity")]
    Task<IReadOnlyCollection<DataPointDto>> GetHumidityHistoryAsync([Query] DateTimeOffset from, [Query] DateTimeOffset to, CancellationToken cancellationToken);

    [Get("/history/pressure")]
    Task<IReadOnlyCollection<DataPointDto>> GetPressureHistoryAsync([Query] DateTimeOffset from, [Query] DateTimeOffset to, CancellationToken cancellationToken);
}
