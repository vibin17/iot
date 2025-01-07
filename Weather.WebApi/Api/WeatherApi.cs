using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Runtime.CompilerServices;

using Weather.WebApi.Api.Models;
using Weather.WebApi.Data;

namespace Weather.WebApi.Api;

public static class WeatherApi
{
    public static RouteGroupBuilder MapWeatherApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/weather").WithOpenApi();

        api.MapGet("/current", GetCurrentAsync).WithSummary("Получить текущие значения погодных показателей").WithOpenApi();
        api.MapGet("/history/temperature", GetTemperatureHistoryAsync).WithSummary("Получить историю изменения температуры").WithOpenApi();
        api.MapGet("/history/humidity", GetHumidityHistoryAsync).WithSummary("Получить историю изменения влажности").WithOpenApi();
        api.MapGet("/history/pressure", GetPressureHistoryAsync).WithSummary("Получить историю изменения давления").WithOpenApi();

        return api;
    }

    public static async Task<Ok<WeatherSnapshotDto>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var current = new WeatherSnapshotDto
        {
            Timestamp = DateTimeOffset.Now,
            Temperature = -5,
            Humidity = 86,
            Pressure = 754 * 1.33322
        };

        return TypedResults.Ok(current);
    }

    public static async Task<Ok<IReadOnlyCollection<DataPointDto>>> GetTemperatureHistoryAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] WeatherContext weatherContext,
        CancellationToken cancellationToken)
    {
        var points = await GetHistoryAsyncCore(
            from, 
            to, 
            "Temperature", 
            weatherContext, 
            cancellationToken);

        return TypedResults.Ok(points);
    }

    public static async Task<Ok<IReadOnlyCollection<DataPointDto>>> GetHumidityHistoryAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] WeatherContext weatherContext,
        CancellationToken cancellationToken)
    {
        var points = await GetHistoryAsyncCore(
            from,
            to,
            "Humidity",
            weatherContext,
            cancellationToken);

        return TypedResults.Ok(points);
    }

    public static async Task<Ok<IReadOnlyCollection<DataPointDto>>> GetPressureHistoryAsync(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromServices] WeatherContext weatherContext,
        CancellationToken cancellationToken)
    {
        var points = await GetHistoryAsyncCore(
            from,
            to,
            "Pressure",
            weatherContext,
            cancellationToken);

        return TypedResults.Ok(points);
    }

    private static async Task<IReadOnlyCollection<DataPointDto>> GetHistoryAsyncCore(
        DateTimeOffset from,
        DateTimeOffset to,
        string columnName,
        WeatherContext context,
        CancellationToken cancellationToken)
    {
        var raw = $$"""
            SELECT time as "Timestamp", value as "Value" FROM unnest(
                (SELECT asap_smooth("Timestamp", "{{columnName}}", 100)
                    FROM (
                        SELECT "Timestamp", "{{columnName}}" FROM "WeatherSnapshots"
                        WHERE "Timestamp" > {0} and "Timestamp" < {1})))
        """;

        var sql =  FormattableStringFactory.Create(raw, from.ToUniversalTime(), to.ToUniversalTime());

        return await context.Database
            .SqlQuery<DataPointDto>(sql)
            .ToListAsync(cancellationToken);
    }
}
