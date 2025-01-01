using Refit;

using Weather.ClientApp.Models;

namespace Weather.ClientApp.Api;

public interface IWeatherApi
{
    [Get("/current")]
    Task<WeatherSnapshotDto> GetCurrentAsync(CancellationToken cancellationToken);

    [Get("/history/temperature")]
    Task<DataPointDto> GetTemperatureHistoryAsync(CancellationToken cancellationToken);

    [Get("/history/humidity")]
    Task<DataPointDto> GetHumidityHistoryAsync(CancellationToken cancellationToken);

    [Get("/history/pressure")]
    Task<DataPointDto> GetPressureHistoryAsync(CancellationToken cancellationToken);
}
