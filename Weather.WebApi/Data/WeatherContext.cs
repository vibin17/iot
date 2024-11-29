using Microsoft.EntityFrameworkCore;

namespace Weather.WebApi.Data;

public class WeatherContext : DbContext
{
    public required DbSet<WeatherEntry> WeatherEntries { get; init; }

    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<WeatherEntry>();

        builder.HasNoKey();
        
        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.Temperature).IsRequired();
        builder.Property(e => e.Humidity).IsRequired();
        builder.Property(e => e.Pressure).IsRequired();
    }
}
