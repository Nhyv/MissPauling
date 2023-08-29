using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class SuggestionVoting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal[]>(
                name: "downvote_users",
                table: "suggestions",
                type: "numeric(20,0)[]",
                nullable: true);

            migrationBuilder.AddColumn<decimal[]>(
                name: "upvote_users",
                table: "suggestions",
                type: "numeric(20,0)[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "downvote_users",
                table: "suggestions");

            migrationBuilder.DropColumn(
                name: "upvote_users",
                table: "suggestions");
        }
    }
}
