using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_settings_row_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""insert into "user".settings default values;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""delete from "user".settings;""");
        }
    }
}
