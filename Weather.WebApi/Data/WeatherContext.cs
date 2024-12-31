using Microsoft.EntityFrameworkCore;

namespace Weather.WebApi.Data;

public class WeatherContext : DbContext
{
    public required DbSet<WeatherSnapshot> WeatherSnapshots { get; init; }

    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<WeatherSnapshot>();

        builder.HasNoKey();
        
        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.Temperature).IsRequired();
        builder.Property(e => e.Humidity).IsRequired();
        builder.Property(e => e.Pressure).IsRequired();
    }
}
