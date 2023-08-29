using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class Contest_ResultsUntil : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "state",
                table: "contests");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "allow_results_viewing_until",
                table: "contests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allow_results_viewing_until",
                table: "contests");

            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "contests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
