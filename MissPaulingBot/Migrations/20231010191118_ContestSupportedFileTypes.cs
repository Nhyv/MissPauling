using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissPaulingBot.Migrations
{
    /// <inheritdoc />
    public partial class ContestSupportedFileTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries");

            migrationBuilder.AlterColumn<decimal>(
                name: "moderator_id",
                table: "verification_entries",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "verification_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<List<string>>(
                name: "accepted_file_types",
                table: "contests",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries");

            migrationBuilder.DropColumn(
                name: "id",
                table: "verification_entries");

            migrationBuilder.DropColumn(
                name: "accepted_file_types",
                table: "contests");

            migrationBuilder.AlterColumn<decimal>(
                name: "moderator_id",
                table: "verification_entries",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries",
                column: "steam_id");
        }
    }
}
