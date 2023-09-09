using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.CreateTable(
                name: "settings",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    default_currency = table.Column<string>(type: "varchar", nullable: false, defaultValueSql: "'RUB'"),
                    currency_round_count = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "2")
                },
                constraints: table =>
                {
                    table.PrimaryKey("settings_row_pk", x => x.id);
                    table.CheckConstraint("only_one_row_ch", "id = 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "settings",
                schema: "user");
        }
    }
}
