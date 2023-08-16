using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_CurrencyInfo_pk_to_composite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "currency_info_pk",
                schema: "cur",
                table: "currency_info");

            migrationBuilder.AddPrimaryKey(
                name: "currency_info_pk",
                schema: "cur",
                table: "currency_info",
                columns: new[] { "code", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "currency_info_pk",
                schema: "cur",
                table: "currency_info");

            migrationBuilder.AddPrimaryKey(
                name: "currency_info_pk",
                schema: "cur",
                table: "currency_info",
                column: "code");
        }
    }
}
