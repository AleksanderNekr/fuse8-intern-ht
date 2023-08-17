using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
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
    }
}
