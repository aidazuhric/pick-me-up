using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class RequireTwoFactorForAdminLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "TwoFactorMode",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "TwoFactorMode",
                value: 0);
        }
    }
}
