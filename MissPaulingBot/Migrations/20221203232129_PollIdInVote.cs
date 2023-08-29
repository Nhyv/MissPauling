using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class PollIdInVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "poll_id",
                table: "poll_votes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "poll_id",
                table: "poll_votes");
        }
    }
}
