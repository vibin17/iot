using Microsoft.EntityFrameworkCore;

using Weather.WebApi.Api;
using Weather.WebApi.Background;
using Weather.WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WeatherContext>(
    c => c.UseNpgsql(
        "Host=localhost;Port=5432;Database=weather;Password=postgres;Username=postgres", 
        pgsql => pgsql.MigrationsAssembly(typeof(Program).Assembly.FullName)));

builder.Services.AddSingleton<ISnapshots, Snapshots>();

builder.Services.AddHostedService<Seeder>();
builder.Services.AddHostedService<PollingWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapWeatherApi();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();

await dbContext.Database.MigrateAsync();

app.Run();