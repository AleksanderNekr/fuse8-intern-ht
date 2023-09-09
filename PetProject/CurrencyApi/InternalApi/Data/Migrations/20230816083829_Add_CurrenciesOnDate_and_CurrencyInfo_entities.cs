using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_CurrenciesOnDate_and_CurrencyInfo_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cur");

            migrationBuilder.CreateTable(
                name: "currencies_on_date",
                schema: "cur",
                columns: table => new
                {
                    last_updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cur_on_date_pk", x => x.last_updated_at);
                    table.CheckConstraint("date_range_ch", "last_updated_at <= now()");
                });

            migrationBuilder.CreateTable(
                name: "currency_info",
                schema: "cur",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar", nullable: false),
                    value = table.Column<decimal>(type: "decimal", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currency_info_pk", x => x.code);
                    table.CheckConstraint("currency_info_code_enum_ch", "code IN ('USD', 'RUB', 'KZT', 'EUR')");
                    table.ForeignKey(
                        name: "currencies_on_date_have_infos_fk",
                        column: x => x.UpdatedAt,
                        principalSchema: "cur",
                        principalTable: "currencies_on_date",
                        principalColumn: "last_updated_at",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "currency_info_updated_at_idx",
                schema: "cur",
                table: "currency_info",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_info",
                schema: "cur");

            migrationBuilder.DropTable(
                name: "currencies_on_date",
                schema: "cur");
        }
    }
}
