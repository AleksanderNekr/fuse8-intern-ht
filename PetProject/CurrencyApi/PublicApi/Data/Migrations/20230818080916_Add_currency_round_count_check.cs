using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_currency_round_count_check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table "user".settings
                                 add constraint "currency_round_count_range_ch"
                                 check ( currency_round_count >= 0 and currency_round_count <= 28 );
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table "user".settings
                                 drop constraint "currency_round_count_range_ch";
                                 """);
        }
    }
}
