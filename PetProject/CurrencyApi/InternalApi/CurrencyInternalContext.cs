using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi;

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
    }
}
