using Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.EntitiesConfiguration;

public class FavExchangeRateConfig : IEntityTypeConfiguration<FavoriteExchangeRateEntity>
{
    public void Configure(EntityTypeBuilder<FavoriteExchangeRateEntity> builder)
    {
        builder.Property(static entity => entity.Name)
               .HasColumnName("name")
               .HasMaxLength(255);

        builder.Property(static entity => entity.Currency)
               .HasConversion(static currency => currency.ToString(),
                              static s => Enum.Parse<CurrencyType>(s, true))
               .HasColumnName("currency")
               .HasColumnType("varchar");

        builder.Property(static entity => entity.BaseCurrency)
               .HasConversion(static currency => currency.ToString(),
                              static s => Enum.Parse<CurrencyType>(s, true))
               .HasColumnName("base_currency")
               .HasColumnType("varchar");

        builder.Property<int>("Id")
               .HasColumnName("id")
               .IsRequired()
               .UseSerialColumn();

        builder.HasKey("Id")
               .HasName("fav_exch_rate_pk");

        builder.HasIndex(static entity => entity.Name)
               .IsUnique()
               .HasDatabaseName("fav_exch_rate_name_uq");

        builder.HasIndex(static entity => new { entity.Currency, entity.BaseCurrency })
               .IsUnique()
               .HasDatabaseName("currency_and_base_currency_uq");

        builder.ToTable("favorite_exchange_rates",
                        static tableBuilder =>
                        {
                            tableBuilder.HasCheckConstraint("currencies_enum_range_ch",
                                                            "currency IN ('USD', 'RUB', 'KZT', 'EUR')"
                                                          + " and base_currency IN ('USD', 'RUB', 'KZT', 'EUR')");
                        });
    }
}
