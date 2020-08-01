using Microsoft.EntityFrameworkCore.Migrations;

namespace Masny.WebApi.Migrations
{
    public partial class RemoveIpFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByIp",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "RevokedByIp",
                table: "RefreshToken");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevokedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
