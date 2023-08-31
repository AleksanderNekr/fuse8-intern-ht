﻿// <auto-generated />
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    [DbContext(typeof(CurrencyPublicContext))]
    [Migration("20230818080916_Add_currency_round_count_check")]
    partial class Add_currency_round_count_check
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("user")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

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