using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class ContestDJBitchedAbout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contest_settings");

            migrationBuilder.DropTable(
                name: "submissions");

            migrationBuilder.DropTable(
                name: "votes");

            migrationBuilder.CreateTable(
                name: "contest_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    creator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    extension = table.Column<string>(type: "text", nullable: true),
                    data = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contest_votes",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    contest_id = table.Column<int>(type: "integer", nullable: false),
                    submission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_votes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    allow_submissions_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    allow_submissions_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    allow_voting_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    allow_voting_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    allow_results_viewing_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contests", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contest_submissions");

            migrationBuilder.DropTable(
                name: "contest_votes");

            migrationBuilder.DropTable(
                name: "contests");

            migrationBuilder.CreateTable(
                name: "contest_settings",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_result_viewing = table.Column<bool>(type: "boolean", nullable: false),
                    allow_submissions = table.Column<bool>(type: "boolean", nullable: false),
                    banner_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    icon_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_settings", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    creation_url = table.Column<string>(type: "text", nullable: true),
                    creator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    submission_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    banner_vote = table.Column<int>(type: "integer", nullable: false),
                    icon_vote = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_votes", x => x.user_id);
                });
        }
    }
}
