using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.EntitiesConfigurations;

public class CurrencyInfoConfig : IEntityTypeConfiguration<CurrencyInfoEntity>
{
    public void Configure(EntityTypeBuilder<CurrencyInfoEntity> builder)
    {
        builder.Property(static entity => entity.Code)
               .HasConversion(static @enum => @enum.ToString(),
                              static value => Enum.Parse<CurrencyType>(value, true))
               .HasColumnName("code")
               .HasColumnType("varchar");

        builder.Property(static entity => entity.Value)
               .HasColumnName("value")
               .HasColumnType("decimal");

        builder.Property(static entity => entity.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz");

        builder.HasOne(static currInfo => currInfo.CurrenciesOnDate)
               .WithMany(static currOnDate => currOnDate.Currencies)
               .HasForeignKey(static currInfo => currInfo.UpdatedAt)
               .HasConstraintName("currencies_on_date_have_infos_fk")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(static entity => new { entity.Code, entity.UpdatedAt })
               .HasName("currency_info_pk");

        builder.ToTable("currency_info",
                        static tableBuilder => tableBuilder.HasCheckConstraint("cur_value_positive_ch",
                                                                               "value >= 0"));
    }
}
