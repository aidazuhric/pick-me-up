using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedImagePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoImagePath",
                value: "car-photos/1.jpg");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhotoImagePath",
                value: "car-photos/2.jpg");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoImagePath",
                value: "car-photos/3.jpg");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4,
                column: "PhotoImagePath",
                value: "car-photos/4.jpg");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 5,
                column: "PhotoImagePath",
                value: "car-photos/5.jpg");

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 6,
                column: "PhotoImagePath",
                value: "car-photos/6.jpg");

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogoImagePath",
                value: "company-logos/1.png");

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 2,
                column: "LogoImagePath",
                value: "company-logos/2.png");

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 3,
                column: "LogoImagePath",
                value: "company-logos/3.png");

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 4,
                column: "LogoImagePath",
                value: "company-logos/4.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProfileImagePath",
                value: "user-photos/1.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProfileImagePath",
                value: "user-photos/2.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProfileImagePath",
                value: "user-photos/3.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProfileImagePath",
                value: "user-photos/4.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "ProfileImagePath",
                value: "user-photos/5.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "ProfileImagePath",
                value: "user-photos/6.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "ProfileImagePath",
                value: "user-photos/7.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                column: "ProfileImagePath",
                value: "user-photos/8.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9,
                column: "ProfileImagePath",
                value: "user-photos/9.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                column: "ProfileImagePath",
                value: "user-photos/10.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                column: "ProfileImagePath",
                value: "user-photos/11.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                column: "ProfileImagePath",
                value: "user-photos/12.jpg");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13,
                column: "ProfileImagePath",
                value: "user-photos/13.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 5,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 6,
                column: "PhotoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 2,
                column: "LogoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 3,
                column: "LogoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 4,
                column: "LogoImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                column: "ProfileImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13,
                column: "ProfileImagePath",
                value: null);
        }
    }
}
