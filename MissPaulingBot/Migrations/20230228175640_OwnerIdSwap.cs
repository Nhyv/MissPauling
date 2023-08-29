using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MissPaulingBot.Migrations
{
    public partial class OwnerIdSwap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_saxton_own_roles",
                table: "saxton_own_roles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_saxton_own_roles",
                table: "saxton_own_roles",
                column: "owner_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_saxton_own_roles",
                table: "saxton_own_roles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_saxton_own_roles",
                table: "saxton_own_roles",
                column: "id");
        }
    }
}
