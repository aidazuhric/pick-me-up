using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxiCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxiCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HqAddress = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StartingPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PricePerKm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LogoImagePath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxiCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxiCompanies_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxiCompanies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TaxiCompanies",
                columns: new[] { "Id", "CityId", "ContactEmail", "ContactPhone", "CreatedAt", "DeletedAt", "HqAddress", "IsActive", "IsDeleted", "LogoImagePath", "Name", "OwnerName", "PricePerKm", "StartingPrice", "UpdatedAt", "UserId" },
                values: new object[] { 1, 1, "info@mostartaxi.local", "+38736111222", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bulevar 12", true, false, null, "Mostar Taxi", "Bella Company", 1.20m, 2.50m, null, 2 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Email", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "ProfileImagePath", "Username" },
                values: new object[,]
                {
                    { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "company2@pickmeup.local", "Nedim", true, false, null, "Neretva", "AwJYxEEx8ikPN8F8vgy2oKAcjvE=", "Li1IhzglTwqEbHNNdr/PuQ==", null, null, "company2" },
                    { 6, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "company3@pickmeup.local", "Selma", true, false, null, "Express", "AwJYxEEx8ikPN8F8vgy2oKAcjvE=", "Li1IhzglTwqEbHNNdr/PuQ==", null, null, "company3" }
                });

            migrationBuilder.InsertData(
                table: "TaxiCompanies",
                columns: new[] { "Id", "CityId", "ContactEmail", "ContactPhone", "CreatedAt", "DeletedAt", "HqAddress", "IsActive", "IsDeleted", "LogoImagePath", "Name", "OwnerName", "PricePerKm", "StartingPrice", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 2, 1, "info@neretvaprevoz.local", "+38736333444", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kralja Tomislava 4", true, false, null, "Neretva Prevoz", "Nedim Neretva", 1.35m, 2.00m, null, 5 },
                    { 3, 2, "info@sarajevoexpress.local", "+38733555666", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "Zmaja od Bosne 88", true, false, null, "Sarajevo Express", "Selma Express", 1.10m, 3.00m, null, 6 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5 },
                    { 6, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 2, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxiCompanies_CityId",
                table: "TaxiCompanies",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiCompanies_Name",
                table: "TaxiCompanies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxiCompanies_UserId",
                table: "TaxiCompanies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxiCompanies");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
