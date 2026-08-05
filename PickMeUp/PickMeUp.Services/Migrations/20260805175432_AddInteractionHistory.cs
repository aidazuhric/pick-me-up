using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TaxiCompanyId = table.Column<int>(type: "integer", nullable: false),
                    SignalTypeId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InteractionHistories_TaxiCompanies_TaxiCompanyId",
                        column: x => x.TaxiCompanyId,
                        principalTable: "TaxiCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteractionHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "TaxiCompanyId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 11, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 1, new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), null, "Bulevar 22", 43.350200000000001, 17.814, 3.2m, 1, 6.34m, false, 4, 4, new DateTime(2026, 6, 11, 9, 12, 0, 0, DateTimeKind.Utc), "Kralja Tomislava 3", 43.343800000000002, 17.8078, 1, new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 6, 18, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 1, new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), null, "Rondo 1", 43.3489, 17.799499999999998, 5.6m, 1, 9.22m, false, 4, 4, new DateTime(2026, 6, 18, 9, 12, 0, 0, DateTimeKind.Utc), "Marsala Tita 40", 43.3401, 17.813500000000001, 1, new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 6, 25, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 2, new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), null, "Aerodrom Mostar", 43.282899999999998, 17.8459, 8.4m, 2, 12.58m, false, 4, 4, new DateTime(2026, 6, 25, 9, 12, 0, 0, DateTimeKind.Utc), "Splitska 5", 43.335000000000001, 17.802099999999999, 1, new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2026, 7, 2, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 1, new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), null, "Stari Most", 43.337400000000002, 17.815100000000001, 4.1m, 1, 7.42m, false, 4, 4, new DateTime(2026, 7, 2, 9, 12, 0, 0, DateTimeKind.Utc), "Bulevar 22", 43.350200000000001, 17.814, 1, new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2026, 7, 9, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 2, new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), null, "Kampus Sveucilista", 43.341900000000003, 17.804300000000001, 6.9m, 2, 10.78m, false, 4, 4, new DateTime(2026, 7, 9, 9, 12, 0, 0, DateTimeKind.Utc), "Kneza Domagoja 12", 43.347700000000003, 17.796099999999999, 1, new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Email", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "ProfileImagePath", "Username" },
                values: new object[,]
                {
                    { 9, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "passenger2@pickmeup.local", "Amar", true, false, null, "Selimovic", "56VoVY84Uv3DwxLzx4fwlU3IzrM=", "Rk9wYmJ2MnhLd0RlTHRRUQ==", "+38761234567", null, "passenger2" },
                    { 10, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "passenger3@pickmeup.local", "Ines", true, false, null, "Maric", "pG3YU6itVrSMloQBg/eTd0NOfNo=", "c1RtWnFYaDdOdkxrRWJVdA==", "+38761765432", null, "passenger3" },
                    { 11, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "company4@pickmeup.local", "Kenan", true, false, null, "Bosna", "VryTRHSCv/ouQq2T4UhDu7VG8ow=", "WGpQdzRzTmhCcTJmVmRLYQ==", "+38733555666", null, "company4" },
                    { 12, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "driver4@pickmeup.local", "Tarik", true, false, null, "Zovko", "NVrK8OXfEs43X/SeZ5ylkNrLeMg=", "THBjNXZLZ1J3TjhtWXNFag==", "+38736777888", null, "driver4" },
                    { 13, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "driver5@pickmeup.local", "Adna", true, false, null, "Hadzic", "Yk+hY2PFplbc+iJLzo9GXPyQCXo=", "QjdrTndSdFhoTTJ2Q3BZZA==", "+38733999000", null, "driver5" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "HireDate", "IsActive", "IsDeleted", "ResignedAt", "TaxiCompanyId", "UpdatedAt", "UserId" },
                values: new object[] { 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, 2, null, 12 });

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), 1, 1, 1, 4, 1m },
                    { 2, new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc), 1, 2, 1, 4, 1m },
                    { 3, new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), 2, 1, 1, 4, 1m },
                    { 4, new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc), 2, 2, 1, 4, 1m },
                    { 5, new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), 3, 1, 1, 4, 1m },
                    { 6, new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc), 3, 2, 1, 4, 1m },
                    { 7, new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), 4, 1, 1, 4, 1m },
                    { 8, new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc), 4, 2, 1, 4, 1m },
                    { 9, new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), 5, 1, 1, 4, 1m },
                    { 10, new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc), 5, 2, 1, 4, 1m },
                    { 25, new DateTime(2026, 6, 11, 10, 35, 0, 0, DateTimeKind.Utc), 1, 3, 1, 4, 5m },
                    { 26, new DateTime(2026, 6, 18, 10, 35, 0, 0, DateTimeKind.Utc), 2, 3, 1, 4, 5m },
                    { 27, new DateTime(2026, 6, 25, 10, 35, 0, 0, DateTimeKind.Utc), 3, 3, 1, 4, 4m }
                });

            migrationBuilder.InsertData(
                table: "OrderStatusHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 1, "Order placed", 1 },
                    { 2, new DateTime(2026, 6, 11, 9, 5, 0, 0, DateTimeKind.Utc), 3, 1, 1, "Accepted by driver", 2 },
                    { 3, new DateTime(2026, 6, 11, 9, 12, 0, 0, DateTimeKind.Utc), 3, 2, 1, "Passenger picked up", 3 },
                    { 4, new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc), 3, 3, 1, "Ride completed", 4 },
                    { 5, new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 2, "Order placed", 1 },
                    { 6, new DateTime(2026, 6, 18, 9, 5, 0, 0, DateTimeKind.Utc), 3, 1, 2, "Accepted by driver", 2 },
                    { 7, new DateTime(2026, 6, 18, 9, 12, 0, 0, DateTimeKind.Utc), 3, 2, 2, "Passenger picked up", 3 },
                    { 8, new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc), 3, 3, 2, "Ride completed", 4 },
                    { 9, new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 3, "Order placed", 1 },
                    { 10, new DateTime(2026, 6, 25, 9, 5, 0, 0, DateTimeKind.Utc), 7, 1, 3, "Accepted by driver", 2 },
                    { 11, new DateTime(2026, 6, 25, 9, 12, 0, 0, DateTimeKind.Utc), 7, 2, 3, "Passenger picked up", 3 },
                    { 12, new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc), 7, 3, 3, "Ride completed", 4 },
                    { 13, new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 4, "Order placed", 1 },
                    { 14, new DateTime(2026, 7, 2, 9, 5, 0, 0, DateTimeKind.Utc), 3, 1, 4, "Accepted by driver", 2 },
                    { 15, new DateTime(2026, 7, 2, 9, 12, 0, 0, DateTimeKind.Utc), 3, 2, 4, "Passenger picked up", 3 },
                    { 16, new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc), 3, 3, 4, "Ride completed", 4 },
                    { 17, new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 5, "Order placed", 1 },
                    { 18, new DateTime(2026, 7, 9, 9, 5, 0, 0, DateTimeKind.Utc), 7, 1, 5, "Accepted by driver", 2 },
                    { 19, new DateTime(2026, 7, 9, 9, 12, 0, 0, DateTimeKind.Utc), 7, 2, 5, "Passenger picked up", 3 },
                    { 20, new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc), 7, 3, 5, "Ride completed", 4 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "TaxiCompanyId", "UpdatedAt" },
                values: new object[,]
                {
                    { 8, new DateTime(2026, 6, 12, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 5, new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), null, "Skenderija", 43.856000000000002, 18.411000000000001, 4.5m, 3, 7.95m, false, 4, 9, new DateTime(2026, 6, 12, 9, 12, 0, 0, DateTimeKind.Utc), "Ferhadija 10", 43.859000000000002, 18.420999999999999, 3, new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2026, 6, 20, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 5, new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), null, "Ilidza", 43.828000000000003, 18.309999999999999, 9.2m, 3, 13.12m, false, 4, 9, new DateTime(2026, 6, 20, 9, 12, 0, 0, DateTimeKind.Utc), "Bascarsija", 43.859499999999997, 18.431000000000001, 3, new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2026, 7, 4, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 5, new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), null, "Vogosca", 43.899999999999999, 18.343, 6.1m, 3, 9.71m, false, 4, 9, new DateTime(2026, 7, 4, 9, 12, 0, 0, DateTimeKind.Utc), "Marijin Dvor", 43.856000000000002, 18.402999999999999, 3, new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 11, new DateTime(2026, 7, 11, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 5, new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), null, "Kosevo", 43.868000000000002, 18.413, 3.8m, 3, 7.18m, false, 4, 9, new DateTime(2026, 7, 11, 9, 12, 0, 0, DateTimeKind.Utc), "Grbavica", 43.847999999999999, 18.395, 3, new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "DeletedAt", "IsDeleted", "OrderId", "PassengerId", "Rating", "TargetCompanyId", "TargetDriverId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Quick pickup and a friendly driver.", new DateTime(2026, 6, 11, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 1, 4, 5, 1, 1, null },
                    { 2, "Always on time, never had a problem.", new DateTime(2026, 6, 18, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 2, 4, 5, 1, 1, null },
                    { 3, "Comfortable ride, arrived a few minutes late.", new DateTime(2026, 6, 25, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 3, 4, 4, 1, 2, null }
                });

            migrationBuilder.InsertData(
                table: "TaxiCompanies",
                columns: new[] { "Id", "CityId", "ContactEmail", "ContactPhone", "CreatedAt", "DeletedAt", "HqAddress", "IsActive", "IsDeleted", "LogoImagePath", "Name", "OwnerName", "PricePerKm", "StartingPrice", "UpdatedAt", "UserId" },
                values: new object[] { 4, 2, "info@bascarsijataxi.local", "+38733555666", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, "Mula Mustafe Baseskije 18", true, false, null, "Bascarsija Taxi", "Kenan Bosna", 1.15m, 2.80m, null, 11 });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "DateAssigned", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 9, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 4, 9 },
                    { 10, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 4, 10 },
                    { 11, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 2, 11 },
                    { 12, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3, 12 },
                    { 13, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3, 13 }
                });

            migrationBuilder.InsertData(
                table: "CarAssignments",
                columns: new[] { "Id", "AssignedFrom", "AssignedTo", "CarId", "CreatedAt", "DeletedAt", "DriverId", "IsActive", "IsDeleted", "UpdatedAt" },
                values: new object[] { 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, false, null });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "IsActive", "IsDeleted", "Make", "Model", "PhotoImagePath", "Plate", "Seats", "TaxiCompanyId", "UpdatedAt", "VehicleCategoryId", "Year" },
                values: new object[] { 6, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, true, false, "Renault", "Megane", null, "J12-A-678", 4, 4, null, 1, 2021 });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "HireDate", "IsActive", "IsDeleted", "ResignedAt", "TaxiCompanyId", "UpdatedAt", "UserId" },
                values: new object[] { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), true, false, null, 4, null, 13 });

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[,]
                {
                    { 15, new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), 8, 1, 3, 9, 1m },
                    { 16, new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc), 8, 2, 3, 9, 1m },
                    { 17, new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), 9, 1, 3, 9, 1m },
                    { 18, new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc), 9, 2, 3, 9, 1m },
                    { 19, new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), 10, 1, 3, 9, 1m },
                    { 20, new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc), 10, 2, 3, 9, 1m },
                    { 21, new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), 11, 1, 3, 9, 1m },
                    { 22, new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc), 11, 2, 3, 9, 1m },
                    { 29, new DateTime(2026, 6, 12, 10, 35, 0, 0, DateTimeKind.Utc), 8, 3, 3, 9, 4m },
                    { 30, new DateTime(2026, 6, 20, 10, 35, 0, 0, DateTimeKind.Utc), 9, 3, 3, 9, 5m }
                });

            migrationBuilder.InsertData(
                table: "OrderStatusHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[,]
                {
                    { 29, new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), 9, null, 8, "Order placed", 1 },
                    { 30, new DateTime(2026, 6, 12, 9, 5, 0, 0, DateTimeKind.Utc), 8, 1, 8, "Accepted by driver", 2 },
                    { 31, new DateTime(2026, 6, 12, 9, 12, 0, 0, DateTimeKind.Utc), 8, 2, 8, "Passenger picked up", 3 },
                    { 32, new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc), 8, 3, 8, "Ride completed", 4 },
                    { 33, new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), 9, null, 9, "Order placed", 1 },
                    { 34, new DateTime(2026, 6, 20, 9, 5, 0, 0, DateTimeKind.Utc), 8, 1, 9, "Accepted by driver", 2 },
                    { 35, new DateTime(2026, 6, 20, 9, 12, 0, 0, DateTimeKind.Utc), 8, 2, 9, "Passenger picked up", 3 },
                    { 36, new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc), 8, 3, 9, "Ride completed", 4 },
                    { 37, new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), 9, null, 10, "Order placed", 1 },
                    { 38, new DateTime(2026, 7, 4, 9, 5, 0, 0, DateTimeKind.Utc), 8, 1, 10, "Accepted by driver", 2 },
                    { 39, new DateTime(2026, 7, 4, 9, 12, 0, 0, DateTimeKind.Utc), 8, 2, 10, "Passenger picked up", 3 },
                    { 40, new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc), 8, 3, 10, "Ride completed", 4 },
                    { 41, new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), 9, null, 11, "Order placed", 1 },
                    { 42, new DateTime(2026, 7, 11, 9, 5, 0, 0, DateTimeKind.Utc), 8, 1, 11, "Accepted by driver", 2 },
                    { 43, new DateTime(2026, 7, 11, 9, 12, 0, 0, DateTimeKind.Utc), 8, 2, 11, "Passenger picked up", 3 },
                    { 44, new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc), 8, 3, 11, "Ride completed", 4 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "TaxiCompanyId", "UpdatedAt" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 6, 14, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 4, new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), null, "Zeljeznicka stanica", 43.347999999999999, 17.808499999999999, 5.0m, 4, 8.75m, false, 4, 4, new DateTime(2026, 6, 14, 9, 12, 0, 0, DateTimeKind.Utc), "Fejiceva 9", 43.338999999999999, 17.815799999999999, 2, new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2026, 7, 16, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 4, new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), null, "Blagaj", 43.256999999999998, 17.888999999999999, 7.3m, 4, 11.86m, false, 4, 4, new DateTime(2026, 7, 16, 9, 12, 0, 0, DateTimeKind.Utc), "Rondo 1", 43.3489, 17.799499999999998, 2, new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "DeletedAt", "IsDeleted", "OrderId", "PassengerId", "Rating", "TargetCompanyId", "TargetDriverId", "UpdatedAt" },
                values: new object[,]
                {
                    { 5, "Good driver and a clean car.", new DateTime(2026, 6, 12, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 8, 9, 4, 3, 3, null },
                    { 6, "Excellent from start to finish.", new DateTime(2026, 6, 20, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 9, 9, 5, 3, 3, null }
                });

            migrationBuilder.InsertData(
                table: "CarAssignments",
                columns: new[] { "Id", "AssignedFrom", "AssignedTo", "CarId", "CreatedAt", "DeletedAt", "DriverId", "IsActive", "IsDeleted", "UpdatedAt" },
                values: new object[] { 5, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, false, null });

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[,]
                {
                    { 11, new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), 6, 1, 2, 4, 1m },
                    { 12, new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc), 6, 2, 2, 4, 1m },
                    { 13, new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), 7, 1, 2, 4, 1m },
                    { 14, new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc), 7, 2, 2, 4, 1m },
                    { 28, new DateTime(2026, 6, 14, 10, 35, 0, 0, DateTimeKind.Utc), 6, 3, 2, 4, 2m }
                });

            migrationBuilder.InsertData(
                table: "OrderStatusHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[,]
                {
                    { 21, new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 6, "Order placed", 1 },
                    { 22, new DateTime(2026, 6, 14, 9, 5, 0, 0, DateTimeKind.Utc), 12, 1, 6, "Accepted by driver", 2 },
                    { 23, new DateTime(2026, 6, 14, 9, 12, 0, 0, DateTimeKind.Utc), 12, 2, 6, "Passenger picked up", 3 },
                    { 24, new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc), 12, 3, 6, "Ride completed", 4 },
                    { 25, new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), 4, null, 7, "Order placed", 1 },
                    { 26, new DateTime(2026, 7, 16, 9, 5, 0, 0, DateTimeKind.Utc), 12, 1, 7, "Accepted by driver", 2 },
                    { 27, new DateTime(2026, 7, 16, 9, 12, 0, 0, DateTimeKind.Utc), 12, 2, 7, "Passenger picked up", 3 },
                    { 28, new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc), 12, 3, 7, "Ride completed", 4 }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "TaxiCompanyId", "UpdatedAt" },
                values: new object[] { 12, new DateTime(2026, 6, 28, 9, 5, 0, 0, DateTimeKind.Utc), null, null, 6, new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), null, "Dobrinja", 43.823, 18.338999999999999, 5.4m, 5, 9.01m, false, 4, 9, new DateTime(2026, 6, 28, 9, 12, 0, 0, DateTimeKind.Utc), "Alipasina 14", 43.861499999999999, 18.4055, 4, new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "DeletedAt", "IsDeleted", "OrderId", "PassengerId", "Rating", "TargetCompanyId", "TargetDriverId", "UpdatedAt" },
                values: new object[] { 4, "Waited far too long for the car to show up.", new DateTime(2026, 6, 14, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 6, 4, 2, 2, 4, null });

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[,]
                {
                    { 23, new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), 12, 1, 4, 9, 1m },
                    { 24, new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc), 12, 2, 4, 9, 1m },
                    { 31, new DateTime(2026, 6, 28, 10, 35, 0, 0, DateTimeKind.Utc), 12, 3, 4, 9, 3m }
                });

            migrationBuilder.InsertData(
                table: "OrderStatusHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[,]
                {
                    { 45, new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), 9, null, 12, "Order placed", 1 },
                    { 46, new DateTime(2026, 6, 28, 9, 5, 0, 0, DateTimeKind.Utc), 13, 1, 12, "Accepted by driver", 2 },
                    { 47, new DateTime(2026, 6, 28, 9, 12, 0, 0, DateTimeKind.Utc), 13, 2, 12, "Passenger picked up", 3 },
                    { 48, new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc), 13, 3, 12, "Ride completed", 4 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "DeletedAt", "IsDeleted", "OrderId", "PassengerId", "Rating", "TargetCompanyId", "TargetDriverId", "UpdatedAt" },
                values: new object[] { 7, "Fine ride, but the fare felt high for the distance.", new DateTime(2026, 6, 28, 10, 35, 0, 0, DateTimeKind.Utc), null, false, 12, 9, 3, 4, 5, null });

            migrationBuilder.CreateIndex(
                name: "IX_InteractionHistories_OrderId",
                table: "InteractionHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionHistories_TaxiCompanyId_SignalTypeId",
                table: "InteractionHistories",
                columns: new[] { "TaxiCompanyId", "SignalTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractionHistories_UserId_TaxiCompanyId_SignalTypeId",
                table: "InteractionHistories",
                columns: new[] { "UserId", "TaxiCompanyId", "SignalTypeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractionHistories");

            migrationBuilder.DeleteData(
                table: "CarAssignments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CarAssignments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TaxiCompanies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
