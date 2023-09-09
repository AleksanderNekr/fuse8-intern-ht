using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data;

public class CurrencyPublicContext : DbContext
{
    public CurrencyPublicContext(DbContextOptions<CurrencyPublicContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CurrencyApiConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public DbSet<CurrenciesSettings> Settings { get; set; } = null!;

    public DbSet<FavoriteExchangeRateEntity> FavoriteExchangeRates { get; set; } = null!;
}
