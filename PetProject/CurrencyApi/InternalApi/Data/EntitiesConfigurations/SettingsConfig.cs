using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.EntitiesConfigurations;

/// <inheritdoc />
public class SettingsConfig : IEntityTypeConfiguration<CurrenciesSettings>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CurrenciesSettings> builder)
    {
        builder.Property(static settings => settings.BaseCurrency)
               .HasColumnName("base_currency")
               .HasColumnType("varchar")
               .HasDefaultValueSql("'USD'");

        builder.Property(static settings => settings.MinAvailableYear)
               .HasColumnName("min_available_year")
               .HasDefaultValueSql("2000");

        // Only one row allowed.
        builder.Property<int>("Id")
               .HasColumnName("id")
               .HasDefaultValueSql("0");

        builder.HasKey("Id")
               .HasName("settings_row_pk");

        builder.ToTable("settings",
                        static tableBuilder =>
                        {
                            tableBuilder.HasCheckConstraint("only_one_row_ch", "id = 0");
                            tableBuilder.HasCheckConstraint("currency_enum_range_ch",
                                                            $"base_currency IN ('{CurrencyType.EUR}', '{CurrencyType.KZT}', '{CurrencyType.RUB}', '{CurrencyType.USD}')");
                        });
    }
}
