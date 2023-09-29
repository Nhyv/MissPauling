using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    /// <inheritdoc />
    public partial class VerificationRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_verification_entry",
                table: "verification_entry");

            migrationBuilder.RenameTable(
                name: "verification_entry",
                newName: "verification_entries");

            migrationBuilder.AddPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries",
                column: "steam_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_verification_entries",
                table: "verification_entries");

            migrationBuilder.RenameTable(
                name: "verification_entries",
                newName: "verification_entry");

            migrationBuilder.AddPrimaryKey(
                name: "pk_verification_entry",
                table: "verification_entry",
                column: "steam_id");
        }
    }
}
