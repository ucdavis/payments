using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class AddBackTeamIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TeamId",
                table: "Invoices",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_TeamId",
                table: "Invoices");
        }
    }
}
