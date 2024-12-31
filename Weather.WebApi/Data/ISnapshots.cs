namespace Weather.WebApi.Data;

public interface ISnapshots
{
    Task<WeatherSnapshot> GetAsync(CancellationToken cancellationToken);
}
