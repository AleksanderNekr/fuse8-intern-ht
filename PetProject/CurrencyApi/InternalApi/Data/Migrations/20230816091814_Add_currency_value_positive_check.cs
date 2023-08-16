using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_currency_value_positive_check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "cur_value_positive_ch",
                schema: "cur",
                table: "currency_info",
                sql: "value >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "cur_value_positive_ch",
                schema: "cur",
                table: "currency_info");
        }
    }
}
