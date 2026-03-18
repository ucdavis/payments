using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class TeamSlothAutoApprove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SlothAutoApprove",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlothAutoApprove",
                table: "Teams");
        }
    }
}
