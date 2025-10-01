using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class RechargeAccountApprovals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByKerb",
                table: "RechargeAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByName",
                table: "RechargeAccounts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredByKerb",
                table: "RechargeAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredByName",
                table: "RechargeAccounts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "RechargeAccounts",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RechargeAccounts_ApprovedByKerb",
                table: "RechargeAccounts",
                column: "ApprovedByKerb");

            migrationBuilder.CreateIndex(
                name: "IX_RechargeAccounts_EnteredByKerb",
                table: "RechargeAccounts",
                column: "EnteredByKerb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RechargeAccounts_ApprovedByKerb",
                table: "RechargeAccounts");

            migrationBuilder.DropIndex(
                name: "IX_RechargeAccounts_EnteredByKerb",
                table: "RechargeAccounts");

            migrationBuilder.DropColumn(
                name: "ApprovedByKerb",
                table: "RechargeAccounts");

            migrationBuilder.DropColumn(
                name: "ApprovedByName",
                table: "RechargeAccounts");

            migrationBuilder.DropColumn(
                name: "EnteredByKerb",
                table: "RechargeAccounts");

            migrationBuilder.DropColumn(
                name: "EnteredByName",
                table: "RechargeAccounts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "RechargeAccounts");
        }
    }
}
