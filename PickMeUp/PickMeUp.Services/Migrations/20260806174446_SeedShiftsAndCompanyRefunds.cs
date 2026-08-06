using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedShiftsAndCompanyRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentRefunds",
                columns: new[] { "Id", "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[,]
                {
                    { 3, 3, "The passenger was charged for a longer route than the one driven.", 3.58m, new DateTime(2026, 6, 26, 9, 15, 0, 0, DateTimeKind.Utc), "re_seed_003" },
                    { 4, 4, "The ride was cut short by a breakdown, so the full fare was returned.", 7.42m, new DateTime(2026, 7, 3, 8, 40, 0, 0, DateTimeKind.Utc), "re_seed_004" }
                });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3,
                column: "PaymentStatusId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4,
                column: "PaymentStatusId",
                value: 4);

            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DriverId", "EndedAt", "IsDeleted", "OrdersHandled", "StartedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 11, 7, 30, 0, 0, DateTimeKind.Utc), null, 1, new DateTime(2026, 6, 11, 15, 30, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 11, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 15, 30, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 6, 18, 7, 30, 0, 0, DateTimeKind.Utc), null, 1, new DateTime(2026, 6, 18, 15, 30, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 18, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 15, 30, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 7, 2, 7, 30, 0, 0, DateTimeKind.Utc), null, 1, new DateTime(2026, 7, 2, 16, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 7, 2, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 16, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Utc), null, 2, new DateTime(2026, 6, 25, 16, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 16, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Utc), null, 2, new DateTime(2026, 7, 9, 16, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 16, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), null, 4, new DateTime(2026, 6, 14, 17, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 17, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), null, 4, new DateTime(2026, 7, 16, 17, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 17, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2026, 6, 12, 7, 0, 0, 0, DateTimeKind.Utc), null, 3, new DateTime(2026, 6, 12, 15, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 12, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Utc), null, 3, new DateTime(2026, 6, 20, 15, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2026, 7, 4, 7, 0, 0, 0, DateTimeKind.Utc), null, 3, new DateTime(2026, 7, 4, 15, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 7, 4, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, new DateTime(2026, 7, 11, 7, 0, 0, 0, DateTimeKind.Utc), null, 3, new DateTime(2026, 7, 11, 15, 0, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 7, 11, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, new DateTime(2026, 6, 28, 8, 30, 0, 0, DateTimeKind.Utc), null, 5, new DateTime(2026, 6, 28, 16, 30, 0, 0, DateTimeKind.Utc), false, 1, new DateTime(2026, 6, 28, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 16, 30, 0, 0, DateTimeKind.Utc) },
                    { 13, new DateTime(2026, 8, 6, 6, 45, 0, 0, DateTimeKind.Utc), null, 1, null, false, 0, new DateTime(2026, 8, 6, 6, 45, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "ShiftBreaks",
                columns: new[] { "Id", "CreatedAt", "EndedAt", "ShiftId", "StartedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 18, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 11, 30, 0, 0, DateTimeKind.Utc), 2, new DateTime(2026, 6, 18, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 11, 30, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 12, 45, 0, 0, DateTimeKind.Utc), 4, new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 12, 45, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 6, 12, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 11, 0, 0, 0, DateTimeKind.Utc), 8, new DateTime(2026, 6, 12, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 11, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3,
                column: "PaymentStatusId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4,
                column: "PaymentStatusId",
                value: 2);
        }
    }
}
