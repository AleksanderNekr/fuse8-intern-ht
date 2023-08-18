﻿// <auto-generated />
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    [DbContext(typeof(CurrencyPublicContext))]
    partial class CurrencyPublicContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("user")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities.FavoriteExchangeRateEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<string>("BaseCurrency")
                        .IsRequired()
                        .HasColumnType("varchar")
                        .HasColumnName("base_currency");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("varchar")
                        .HasColumnName("currency");

                    b.HasKey("Name")
                        .HasName("fav_exch_rate_name_pk");

                    b.HasIndex("Currency", "BaseCurrency")
                        .IsUnique()
                        .HasDatabaseName("currency_and_base_currency_uq");

                    b.ToTable("favorite_exchange_rates", "user", t =>
                        {
                            t.HasCheckConstraint("currencies_enum_range_ch", "currency IN ('USD', 'RUB', 'KZT', 'EUR') and base_currency IN ('USD', 'RUB', 'KZT', 'EUR')");
                        });
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings.CurrenciesSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasDefaultValueSql("0");

                    b.Property<int>("DecimalPlace")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("currency_round_count")
                        .HasDefaultValueSql("2");

                    b.Property<string>("DefaultCurrency")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar")
                        .HasColumnName("default_currency")
                        .HasDefaultValueSql("'RUB'");

                    b.HasKey("Id")
                        .HasName("settings_row_pk");

                    b.ToTable("settings", "user", t =>
                        {
                            t.HasCheckConstraint("only_one_row_ch", "id = 0");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
