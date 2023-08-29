using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class Polls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "polls",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    open_poll_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    remove_voting_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    display_votes_publicly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_polls", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "poll_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    poll_id = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_poll_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_poll_options_polls_poll_id",
                        column: x => x.poll_id,
                        principalTable: "polls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_votes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    option_id = table.Column<int>(type: "integer", nullable: false),
                    voter_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_poll_votes", x => x.id);
                    table.ForeignKey(
                        name: "fk_poll_votes_poll_options_option_id",
                        column: x => x.option_id,
                        principalTable: "poll_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_poll_options_poll_id",
                table: "poll_options",
                column: "poll_id");

            migrationBuilder.CreateIndex(
                name: "ix_poll_votes_option_id",
                table: "poll_votes",
                column: "option_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "poll_votes");

            migrationBuilder.DropTable(
                name: "poll_options");

            migrationBuilder.DropTable(
                name: "polls");
        }
    }
}
