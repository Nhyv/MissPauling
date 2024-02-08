using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    /// <inheritdoc />
    public partial class HandledByInVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "additional_comment",
                table: "verification_entries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "moderator_id",
                table: "verification_entries",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additional_comment",
                table: "verification_entries");

            migrationBuilder.DropColumn(
                name: "moderator_id",
                table: "verification_entries");
        }
    }
}
