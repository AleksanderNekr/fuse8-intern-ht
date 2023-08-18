using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_currency_check_for_default : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table "user".settings
                                 add constraint "currency_range_ch"
                                 check ( default_currency IN ('USD', 'RUB', 'KZT', 'EUR') );
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table "user".settings
                                 drop constraint "currency_range_ch";
                                 """);
        }
    }
}
