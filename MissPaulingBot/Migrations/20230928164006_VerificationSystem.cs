using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    /// <inheritdoc />
    public partial class VerificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "platform_answer",
                table: "streamer_applications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "verification_entry",
                columns: table => new
                {
                    steam_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    entry_type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verification_entry", x => x.steam_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "verification_entry");

            migrationBuilder.DropColumn(
                name: "platform_answer",
                table: "streamer_applications");
        }
    }
}
