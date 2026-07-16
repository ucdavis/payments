using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class ExternalIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_TeamId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Invoices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalIdentifier",
                table: "Invoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLink",
                table: "Invoices",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TeamId_ExternalIdentifier_ExternalId",
                table: "Invoices",
                columns: new[] { "TeamId", "ExternalIdentifier", "ExternalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_TeamId_ExternalIdentifier_ExternalId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExternalIdentifier",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExternalLink",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TeamId",
                table: "Invoices",
                column: "TeamId");
        }
    }
}
