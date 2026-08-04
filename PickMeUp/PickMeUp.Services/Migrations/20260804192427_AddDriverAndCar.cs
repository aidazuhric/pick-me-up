using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverAndCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Make = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Model = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Seats = table.Column<int>(type: "integer", nullable: false),
                    VehicleCategoryId = table.Column<int>(type: "integer", nullable: false),
                    TaxiCompanyId = table.Column<int>(type: "integer", nullable: false),
                    PhotoImagePath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cars_TaxiCompanies_TaxiCompanyId",
                        column: x => x.TaxiCompanyId,
                        principalTable: "TaxiCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cars_VehicleCategories_VehicleCategoryId",
                        column: x => x.VehicleCategoryId,
                        principalTable: "VehicleCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TaxiCompanyId = table.Column<int>(type: "integer", nullable: false),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_TaxiCompanies_TaxiCompanyId",
                        column: x => x.TaxiCompanyId,
                        principalTable: "TaxiCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drivers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "Make", "Model", "PhotoImagePath", "Plate", "Seats", "TaxiCompanyId", "UpdatedAt", "VehicleCategoryId", "Year" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Skoda", "Octavia", null, "A12-T-345", 4, 1, null, 1, 2019 },
                    { 2, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Volkswagen", "Passat", null, "A34-K-567", 4, 1, null, 2, 2021 },
                    { 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Ford", "Tourneo", null, "A56-M-789", 8, 1, null, 3, 2020 },
                    { 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Skoda", "Superb", null, "E56-M-789", 4, 2, null, 2, 2022 },
                    { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Toyota", "Corolla", null, "K78-J-012", 4, 3, null, 3, 2018 }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "HireDate", "IsActive", "IsDeleted", "ResignedAt", "TaxiCompanyId", "UpdatedAt", "UserId" },
                values: new object[] { 1, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, 1, null, 3 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Email", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "ProfileImagePath", "Username" },
                values: new object[,]
                {
                    { 7, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "driver2@pickmeup.local", "Haris", true, false, null, "Hodzic", "AwJYxEEx8ikPN8F8vgy2oKAcjvE=", "Li1IhzglTwqEbHNNdr/PuQ==", null, null, "driver2" },
                    { 8, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "driver3@pickmeup.local", "Lejla", true, false, null, "Kovac", "AwJYxEEx8ikPN8F8vgy2oKAcjvE=", "Li1IhzglTwqEbHNNdr/PuQ==", null, null, "driver3" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "HireDate", "IsActive", "IsDeleted", "ResignedAt", "TaxiCompanyId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, 1, null, 7 },
                    { 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, 3, null, 8 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 7, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3, 7 },
                    { 8, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Plate",
                table: "Cars",
                column: "Plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_TaxiCompanyId",
                table: "Cars",
                column: "TaxiCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_VehicleCategoryId",
                table: "Cars",
                column: "VehicleCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TaxiCompanyId",
                table: "Drivers",
                column: "TaxiCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
