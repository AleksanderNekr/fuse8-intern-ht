﻿// <auto-generated />
using System;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    [DbContext(typeof(CurrencyInternalContext))]
    [Migration("20230902143547_Add_AddedAt_prop_for_CacheTask")]
    partial class Add_AddedAt_prop_for_CacheTask
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("cur")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CacheTaskEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("AddedAt")
                        .HasColumnType("timestamptz")
                        .HasColumnName("added_at");

                    b.Property<string>("NewBaseCurrency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("new_base_currency");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("cache_task_pk");

                    b.ToTable("cache_tasks", "cur", t =>
                        {
                            t.HasCheckConstraint("currency_enum_range_ch", "new_base_currency IN ('EUR', 'KZT', 'RUB', 'USD')");

                            t.HasCheckConstraint("status_enum_range_ch", "status IN ('Created', 'Running', 'RanToCompletion', 'Canceled', 'Faulted')");
                        });
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CurrenciesOnDateEntity", b =>
                {
                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("timestamptz")
                        .HasColumnName("last_updated_at");

                    b.HasKey("LastUpdatedAt")
                        .HasName("cur_on_date_pk");

                    b.ToTable("currencies_on_date", "cur", t =>
                        {
                            t.HasCheckConstraint("date_range_ch", "last_updated_at <= now()");
                        });
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CurrencyInfoEntity", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("varchar")
                        .HasColumnName("code");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamptz")
                        .HasColumnName("updated_at");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal")
                        .HasColumnName("value");

                    b.HasKey("Code", "UpdatedAt")
                        .HasName("currency_info_pk");

                    b.HasIndex("UpdatedAt");

                    b.ToTable("currency_info", "cur", t =>
                        {
                            t.HasCheckConstraint("cur_value_positive_ch", "value >= 0");
                        });
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CurrencyInfoEntity", b =>
                {
                    b.HasOne("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CurrenciesOnDateEntity", "CurrenciesOnDate")
                        .WithMany("Currencies")
                        .HasForeignKey("UpdatedAt")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("currencies_on_date_have_infos_fk");

                    b.Navigation("CurrenciesOnDate");
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities.CurrenciesOnDateEntity", b =>
                {
                    b.Navigation("Currencies");
                });
#pragma warning restore 612, 618
        }
    }
}
