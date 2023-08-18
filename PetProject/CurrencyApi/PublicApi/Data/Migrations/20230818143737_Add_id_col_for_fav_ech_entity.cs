using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_id_col_for_fav_ech_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "fav_exch_rate_name_pk",
                schema: "user",
                table: "favorite_exchange_rates");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "user",
                table: "favorite_exchange_rates",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "fav_exch_rate_pk",
                schema: "user",
                table: "favorite_exchange_rates",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "fav_exch_rate_name_uq",
                schema: "user",
                table: "favorite_exchange_rates",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "fav_exch_rate_pk",
                schema: "user",
                table: "favorite_exchange_rates");

            migrationBuilder.DropIndex(
                name: "fav_exch_rate_name_uq",
                schema: "user",
                table: "favorite_exchange_rates");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "user",
                table: "favorite_exchange_rates");

            migrationBuilder.AddPrimaryKey(
                name: "fav_exch_rate_name_pk",
                schema: "user",
                table: "favorite_exchange_rates",
                column: "name");
        }
    }
}
