using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class NewProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artist_applications",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    upload_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artist_applications", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "blacklisted_users",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    moderator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blacklisted_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "contest_settings",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    banner_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    icon_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_submissions = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contest_settings", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "dm_message_templates",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    response = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    author_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dm_message_templates", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "mod_applications",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    age_response = table.Column<string>(type: "text", nullable: true),
                    availabilities_response = table.Column<string>(type: "text", nullable: true),
                    channels_response = table.Column<string>(type: "text", nullable: true),
                    qualification_response = table.Column<string>(type: "text", nullable: true),
                    reason_response = table.Column<string>(type: "text", nullable: true),
                    personal_response = table.Column<string>(type: "text", nullable: true),
                    buttons_response = table.Column<string>(type: "text", nullable: true),
                    butting_heads_response = table.Column<string>(type: "text", nullable: true),
                    abuse_response = table.Column<string>(type: "text", nullable: true),
                    change_response = table.Column<string>(type: "text", nullable: true),
                    has_applied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mod_applications", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "modmail_verifications",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modmail_verifications", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "server_emojis",
                columns: table => new
                {
                    emoji_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    usage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_server_emojis", x => x.emoji_id);
                });

            migrationBuilder.CreateTable(
                name: "spectators",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    given_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spectators", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "streamer_applications",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    content_answer = table.Column<string>(type: "text", nullable: true),
                    reason_answer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streamer_applications", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    creator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    creation_url = table.Column<string>(type: "text", nullable: true),
                    submission_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    moderator_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    given_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notes", x => x.id);
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artist_applications");

            migrationBuilder.DropTable(
                name: "blacklisted_users");

            migrationBuilder.DropTable(
                name: "contest_settings");

            migrationBuilder.DropTable(
                name: "dm_message_templates");

            migrationBuilder.DropTable(
                name: "mod_applications");

            migrationBuilder.DropTable(
                name: "modmail_verifications");

            migrationBuilder.DropTable(
                name: "server_emojis");

            migrationBuilder.DropTable(
                name: "spectators");

            migrationBuilder.DropTable(
                name: "streamer_applications");

            migrationBuilder.DropTable(
                name: "submissions");

            migrationBuilder.DropTable(
                name: "user_notes");

            migrationBuilder.DropTable(
                name: "votes");
        }
    }
}
