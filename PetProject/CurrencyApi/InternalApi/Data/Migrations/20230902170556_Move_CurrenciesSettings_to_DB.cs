using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Move_CurrenciesSettings_to_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "settings",
                schema: "cur",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    base_currency = table.Column<string>(type: "varchar", nullable: false, defaultValueSql: "'USD'"),
                    min_available_year = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "2000")
                },
                constraints: table =>
                {
                    table.PrimaryKey("settings_row_pk", x => x.id);
                    table.CheckConstraint("currency_enum_range_ch", "base_currency IN ('EUR', 'KZT', 'RUB', 'USD')");
                    table.CheckConstraint("only_one_row_ch", "id = 0");
                });
            
            migrationBuilder.Sql("""insert into "cur".settings default values;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "settings",
                schema: "cur");
        }
    }
}
