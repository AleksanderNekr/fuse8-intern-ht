using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_fav_exch_rate_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favorite_exchange_rates",
                schema: "user",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    currency = table.Column<string>(type: "varchar", nullable: false),
                    base_currency = table.Column<string>(type: "varchar", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("fav_exch_rate_name_pk", x => x.name);
                    table.CheckConstraint("currencies_enum_range_ch", "currency IN ('USD', 'RUB', 'KZT', 'EUR') and base_currency IN ('USD', 'RUB', 'KZT', 'EUR')");
                });

            migrationBuilder.CreateIndex(
                name: "currency_and_base_currency_uq",
                schema: "user",
                table: "favorite_exchange_rates",
                columns: new[] { "currency", "base_currency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorite_exchange_rates",
                schema: "user");
        }
    }
}
