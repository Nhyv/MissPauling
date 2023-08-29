using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class StickyRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "spectators");

            migrationBuilder.CreateTable(
                name: "sticky_roles",
                columns: table => new
                {
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    role_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sticky_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "sticky_users",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sticky_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "sticky_role_sticky_user",
                columns: table => new
                {
                    sticky_roles_role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    sticky_users_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sticky_role_sticky_user", x => new { x.sticky_roles_role_id, x.sticky_users_user_id });
                    table.ForeignKey(
                        name: "fk_sticky_role_sticky_user_sticky_roles_sticky_roles_temp_id",
                        column: x => x.sticky_roles_role_id,
                        principalTable: "sticky_roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sticky_role_sticky_user_sticky_users_sticky_users_temp_id",
                        column: x => x.sticky_users_user_id,
                        principalTable: "sticky_users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sticky_role_sticky_user_sticky_users_user_id",
                table: "sticky_role_sticky_user",
                column: "sticky_users_user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sticky_role_sticky_user");

            migrationBuilder.DropTable(
                name: "sticky_roles");

            migrationBuilder.DropTable(
                name: "sticky_users");

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
        }
    }
}
