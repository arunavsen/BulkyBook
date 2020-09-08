using Microsoft.EntityFrameworkCore.Migrations;

namespace BulkyBook.DataAccess.Migrations
{
    public partial class ChangeCompanyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAuthorizedPhoneNumber",
                table: "Companies");

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorizedCompany",
                table: "Companies",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAuthorizedCompany",
                table: "Companies");

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorizedPhoneNumber",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
