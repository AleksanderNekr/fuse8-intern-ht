using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.EntitiesConfigurations;

public class CurrenciesOnDateConfig : IEntityTypeConfiguration<CurrenciesOnDateEntity>
{
    public void Configure(EntityTypeBuilder<CurrenciesOnDateEntity> builder)
    {
        builder.Property(static currencies => currencies.LastUpdatedAt)
               .HasColumnName("last_updated_at")
               .HasColumnType("timestamptz");

        builder.HasKey(static currencies => currencies.LastUpdatedAt)
               .HasName("cur_on_date_pk");

        builder.ToTable("currencies_on_date",
                        static tableBuilder => tableBuilder.HasCheckConstraint("date_range_ch",
                                                                               "last_updated_at <= now()"));
    }
}
