using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.EntitiesConfiguration;

public class CurrenciesSettingsConfig : IEntityTypeConfiguration<CurrenciesSettings>
{
    public void Configure(EntityTypeBuilder<CurrenciesSettings> builder)
    {
        builder.Property(static settings => settings.DefaultCurrency)
               .HasColumnName("default_currency")
               .HasColumnType("varchar")
               .HasDefaultValueSql("'RUB'");

        builder.Property(static settings => settings.DecimalPlace)
               .HasColumnName("currency_round_count")
               .HasDefaultValueSql("2");

        // Only one row allowed.
        builder.Property<int>("Id")
               .HasColumnName("id")
               .HasDefaultValueSql("0");

        builder.HasKey("Id")
               .HasName("settings_row_pk");

        builder.ToTable("settings",
                        static tableBuilder => tableBuilder.HasCheckConstraint("only_one_row_ch",
                                                                               "id = 0"));
    }
}
