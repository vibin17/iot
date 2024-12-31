using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

using Weather.WebApi.Api.Models;
using Weather.WebApi.Data;

namespace Weather.WebApi.Api;

public static class WeatherApi
{
    public static RouteGroupBuilder MapWeatherApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/weather");

        api.MapGet("/current", GetCurrentAsync);
        api.MapGet("/history/temperature", GetTemperatureHistoryAsync);
        api.MapGet("/history/humidity", GetHumidityHistoryAsync);
        api.MapGet("/history/pressure", GetPressureHistoryAsync);

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
            e => new DataPointDto 
            { 
                Timestamp = e.Timestamp, 
                Value = e.Temperature 
            }, 
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
            e => new DataPointDto
            {
                Timestamp = e.Timestamp,
                Value = e.Humidity
            },
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
            e => new DataPointDto
            {
                Timestamp = e.Timestamp,
                Value = e.Pressure
            },
            weatherContext,
            cancellationToken);

        return TypedResults.Ok(points);
    }

    private static async Task<IReadOnlyCollection<DataPointDto>> GetHistoryAsyncCore(
        DateTimeOffset from, 
        DateTimeOffset to,
        Expression<Func<WeatherSnapshot, DataPointDto>> selector,
        WeatherContext context,
        CancellationToken cancellationToken)
    {
        return await context.Database.SqlQuery<DataPointDto>($"""
            SELECT time, value
            FROM unnest((
            SELECT lttb(date, reading, 8)
            FROM metrics))
            """)
            .ToListAsync(cancellationToken);
    }
}
