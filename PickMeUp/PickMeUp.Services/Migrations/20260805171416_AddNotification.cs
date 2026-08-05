using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "Body", "Category", "CreatedAt", "DeletedAt", "IsDeleted", "IsRead", "OrderId", "ReadAt", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, "Order your first ride straight from the home screen.", "order-status", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, null, "Welcome to PickMeUp", 4 },
                    { 2, "Your card is ready, so paying for a ride now takes one tap.", "payment", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Payment method saved", 4 },
                    { 3, "Reason: the driver could not reach the pick-up point.", "order-cancelled", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, true, null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), "A ride was cancelled", 4 },
                    { 4, "Pick-up at Kralja Tomislava 1.", "order-available", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, null, "New ride available", 3 },
                    { 5, "You completed four rides on your last shift.", "order-status", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, false, null, null, "Your shift summary is ready", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrderId",
                table: "Notifications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
