using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data;

/// <inheritdoc />
public class CurrencyInternalContext : DbContext
{
    /// <inheritdoc />
    public CurrencyInternalContext(DbContextOptions<CurrencyInternalContext> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CurrencyApiConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    /// <summary>
    /// Коллекция сущностей валют на определенную дату.
    /// </summary>
    public DbSet<CurrenciesOnDateEntity> CurrenciesOnDates { get; set; } = null!;

    /// <summary>
    /// Коллекция сущностей валют.
    /// </summary>
    public DbSet<CurrencyInfoEntity> CurrencyInfos { get; set; } = null!;

    /// <summary>
    /// Коллекция сущностей кэша задач.
    /// </summary>
    public DbSet<CacheTaskEntity> CacheTasks { get; set; } = null!;
}
