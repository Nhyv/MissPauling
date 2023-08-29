using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class AllowViewing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allow_result_viewing",
                table: "contest_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allow_result_viewing",
                table: "contest_settings");
        }
    }
}
