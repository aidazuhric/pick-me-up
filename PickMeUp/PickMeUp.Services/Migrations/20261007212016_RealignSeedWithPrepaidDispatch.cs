using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class RealignSeedWithPrepaidDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CityId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "IsPrepaid", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "PrepaidAt", "TaxiCompanyId", "UpdatedAt" },
                values: new object[] { 19, null, null, null, null, 1, null, new DateTime(2026, 10, 4, 18, 0, 0, 0, DateTimeKind.Utc), null, "Stari Most", 43.337400000000002, 17.815100000000001, 2.40m, null, 5.24m, false, false, 1, 10, null, "Kneza Domagoja 12", 43.347700000000003, 17.796099999999999, null, 2, null });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "AmountCharged", "CreatedAt", "Currency", "DeletedAt", "IsDeleted", "IsPaid", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[,]
                {
                    { 12, 9.01m, new DateTime(2026, 9, 24, 14, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 12, new DateTime(2026, 9, 24, 14, 2, 0, 0, DateTimeKind.Utc), 4, "pi_seed_order_012", new DateTime(2026, 9, 24, 14, 2, 0, 0, DateTimeKind.Utc) },
                    { 13, 7.31m, new DateTime(2026, 10, 4, 17, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 14, new DateTime(2026, 10, 4, 17, 2, 0, 0, DateTimeKind.Utc), 2, "pi_seed_order_014", new DateTime(2026, 10, 4, 17, 2, 0, 0, DateTimeKind.Utc) },
                    { 14, 10.54m, new DateTime(2026, 10, 3, 9, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 15, new DateTime(2026, 10, 3, 9, 2, 0, 0, DateTimeKind.Utc), 2, "pi_seed_order_015", new DateTime(2026, 10, 3, 9, 2, 0, 0, DateTimeKind.Utc) },
                    { 15, 12.02m, new DateTime(2026, 10, 4, 8, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 16, new DateTime(2026, 10, 4, 8, 2, 0, 0, DateTimeKind.Utc), 2, "pi_seed_order_016", new DateTime(2026, 10, 4, 8, 2, 0, 0, DateTimeKind.Utc) },
                    { 16, 7.78m, new DateTime(2026, 9, 26, 10, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 17, new DateTime(2026, 9, 26, 10, 2, 0, 0, DateTimeKind.Utc), 4, "pi_seed_order_017", new DateTime(2026, 9, 26, 10, 2, 0, 0, DateTimeKind.Utc) },
                    { 17, 9.71m, new DateTime(2026, 9, 28, 11, 1, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 18, new DateTime(2026, 9, 28, 11, 2, 0, 0, DateTimeKind.Utc), 4, "pi_seed_order_018", new DateTime(2026, 9, 28, 11, 2, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DriverId", "EndedAt", "IsDeleted", "OrdersHandled", "StartedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 14, new DateTime(2026, 10, 3, 6, 30, 0, 0, DateTimeKind.Utc), null, 2, null, false, 1, new DateTime(2026, 10, 3, 6, 30, 0, 0, DateTimeKind.Utc), null },
                    { 15, new DateTime(2026, 10, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, 3, null, false, 1, new DateTime(2026, 10, 4, 6, 0, 0, 0, DateTimeKind.Utc), null },
                    { 16, new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc), null, 5, null, false, 0, new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "PaymentRefunds",
                columns: new[] { "Id", "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[,]
                {
                    { 5, 16, "The passenger cancelled before a driver took the ride, so the fare was returned in full.", 7.78m, new DateTime(2026, 9, 26, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_017" },
                    { 6, 17, "The ride was cancelled by the driver, so the fare was returned in full.", 9.71m, new DateTime(2026, 9, 28, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_018" }
                });
        

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 10, 4, 18, 0, 0, 0, DateTimeKind.Utc), 19, 2, 10 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc), 16, 1, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 24, 14, 0, 0, 0, DateTimeKind.Utc), 12, 4, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 24, 14, 35, 0, 0, DateTimeKind.Utc), 12, 4, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[] { new DateTime(2026, 9, 24, 14, 55, 0, 0, DateTimeKind.Utc), 12, 3, 4, 9, 3m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 22, 9, 0, 0, 0, DateTimeKind.Utc), 11, 1, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 22, 9, 35, 0, 0, DateTimeKind.Utc), 11, 2, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 9, 21, 13, 0, 0, 0, DateTimeKind.Utc), 7, 1, 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 9, 21, 13, 35, 0, 0, DateTimeKind.Utc), 7, 2, 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 0, 0, 0, DateTimeKind.Utc), 6, 1, 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 35, 0, 0, DateTimeKind.Utc), 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 55, 0, 0, DateTimeKind.Utc), 3, 2m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 18, 12, 0, 0, 0, DateTimeKind.Utc), 10, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 18, 12, 35, 0, 0, DateTimeKind.Utc), 10, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 17, 11, 0, 0, 0, DateTimeKind.Utc), 5, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 17, 11, 35, 0, 0, DateTimeKind.Utc), 5, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 10, 3, 9, 0, 0, 0, DateTimeKind.Utc), 15, 1, 10 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 15, 8, 0, 0, 0, DateTimeKind.Utc), 4, 1, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 15, 8, 35, 0, 0, DateTimeKind.Utc), 4, 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 9, 14, 10, 0, 0, 0, DateTimeKind.Utc), 9, 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 9, 14, 10, 35, 0, 0, DateTimeKind.Utc), 9, 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 14, 10, 55, 0, 0, DateTimeKind.Utc), 9, 3, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 13, 10, 0, 0, 0, DateTimeKind.Utc), 3, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 13, 10, 35, 0, 0, DateTimeKind.Utc), 3, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "CreatedAt", "OrderId", "Value" },
                values: new object[] { new DateTime(2026, 9, 13, 10, 55, 0, 0, DateTimeKind.Utc), 3, 4m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 11, 9, 0, 0, 0, DateTimeKind.Utc), 1, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 11, 9, 35, 0, 0, DateTimeKind.Utc), 2, 2, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "Value" },
                values: new object[] { new DateTime(2026, 9, 11, 9, 55, 0, 0, DateTimeKind.Utc), 2, 1, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 10, 8, 0, 0, 0, DateTimeKind.Utc), 1, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 10, 8, 35, 0, 0, DateTimeKind.Utc), 8, 2, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "Value" },
                values: new object[] { new DateTime(2026, 9, 10, 8, 55, 0, 0, DateTimeKind.Utc), 8, 3, 4m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "CreatedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 9, 9, 8, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 9, 9, 8, 35, 0, 0, DateTimeKind.Utc), 1, 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 9, 9, 8, 55, 0, 0, DateTimeKind.Utc), 1, 3, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "CreatedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 9, 28, 11, 0, 0, 0, DateTimeKind.Utc), 18 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 26, 10, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ReadAt" },
                values: new object[] { new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ReadAt" },
                values: new object[] { new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 7, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 9, 8, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 9, 8, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 9, 8, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 9, 8, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 11, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 11, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 11, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 8,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 11, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 9,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 13, 10, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 10,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 13, 10, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 11,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 13, 10, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 12,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 13, 10, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 13,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 15, 8, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 14,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 15, 8, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 15,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 15, 8, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 16,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 15, 8, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 17,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 17, 11, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 18,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 17, 11, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 19,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 17, 11, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 20,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 17, 11, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 21,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 19, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 22,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 19, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 23,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 19, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 24,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 19, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 25,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 21, 13, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 26,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 21, 13, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 27,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 21, 13, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 28,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 21, 13, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 29,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 10, 8, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 30,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 10, 8, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 31,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 10, 8, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 32,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 10, 8, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 33,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 14, 10, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 34,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 14, 10, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 35,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 14, 10, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 36,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 14, 10, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 37,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 18, 12, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 38,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 18, 12, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 39,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 18, 12, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 40,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 18, 12, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 41,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 22, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 42,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 22, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 43,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 22, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 44,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 22, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 45,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 24, 14, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 46,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 24, 14, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 47,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 24, 14, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 48,
                column: "ChangedAt",
                value: new DateTime(2026, 9, 24, 14, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ChangedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 9, 26, 10, 0, 0, 0, DateTimeKind.Utc), 17 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 9, 26, 10, 20, 0, 0, DateTimeKind.Utc), 4, 1, 17, "Passenger found another ride before a driver accepted.", 5 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 9, 28, 11, 0, 0, 0, DateTimeKind.Utc), 9, 18 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 9, 28, 11, 5, 0, 0, DateTimeKind.Utc), 8, 18 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 9, 28, 11, 20, 0, 0, DateTimeKind.Utc), 8, 2, 18, "Driver could not reach the pick-up point because of a closed road.", 5 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 3, 9, 0, 0, 0, DateTimeKind.Utc), 10, null, 15, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 3, 9, 5, 0, 0, DateTimeKind.Utc), 7, 1, 15, "Accepted by driver", 2 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc), 9, 16 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 4, 8, 5, 0, 0, DateTimeKind.Utc), 8, 16, "Accepted by driver", 2 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 4, 8, 12, 0, 0, DateTimeKind.Utc), 8, 2, 16, "Passenger picked up", 3 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 4, 17, 0, 0, 0, DateTimeKind.Utc), 9, null, 14, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 10, 4, 18, 0, 0, 0, DateTimeKind.Utc), 10, null, 19, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 9, 8, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 11, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 13, 10, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 15, 8, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 17, 11, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 11, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 11, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 11, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 11, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 21, 13, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 13, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 13, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 13, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 13, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 10, 8, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 8, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 8, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 8, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 8, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 14, 10, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 10, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 10, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 10, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 18, 12, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 12, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 12, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 12, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 12, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 22, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 9, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 24, 14, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 14, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 14, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 14, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 14, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 14, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "Fare", "PrepaidAt", "TaxiCompanyId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 4, 17, 0, 0, 0, DateTimeKind.Utc), 7.31m, new DateTime(2026, 10, 4, 17, 2, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 10, 4, 17, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "AcceptedAt", "CarId", "CreatedAt", "DriverId", "PassengerId", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 3, 9, 5, 0, 0, DateTimeKind.Utc), 2, new DateTime(2026, 10, 3, 9, 0, 0, 0, DateTimeKind.Utc), 2, 10, new DateTime(2026, 10, 3, 9, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 3, 9, 5, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "AcceptedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 4, 8, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 4, 8, 12, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CancelledAt", "CreatedAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 26, 10, 20, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 26, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 26, 10, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 26, 10, 20, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "AcceptedAt", "CancelledAt", "CreatedAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 28, 11, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 11, 20, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 11, 2, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 28, 11, 20, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 3, "The passenger was charged for a longer route than the one driven.", 3.58m, new DateTime(2026, 9, 14, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_003" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 4, "The ride was cut short by a breakdown, so the full fare was returned.", 7.42m, new DateTime(2026, 9, 16, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_004" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 11, "Waiting time was charged in error and returned to the passenger.", 2.18m, new DateTime(2026, 9, 23, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_011" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 12, "The ride was billed twice, so the duplicate charge was returned in full.", 9.01m, new DateTime(2026, 9, 25, 12, 0, 0, 0, DateTimeKind.Utc), "re_seed_order_012" });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 9, 8, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 8, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_001", new DateTime(2026, 9, 9, 8, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 11, 9, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 9, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_002", new DateTime(2026, 9, 11, 9, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 13, 10, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 10, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_003", new DateTime(2026, 9, 13, 10, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 15, 8, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 8, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_004", new DateTime(2026, 9, 15, 8, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsPaid", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 17, 11, 1, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 9, 17, 11, 2, 0, 0, DateTimeKind.Utc), 2, "pi_seed_order_005", new DateTime(2026, 9, 17, 11, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_006", new DateTime(2026, 9, 19, 9, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 11.86m, new DateTime(2026, 9, 21, 13, 1, 0, 0, DateTimeKind.Utc), 7, new DateTime(2026, 9, 21, 13, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_007", new DateTime(2026, 9, 21, 13, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 7.95m, new DateTime(2026, 9, 10, 8, 1, 0, 0, DateTimeKind.Utc), 8, new DateTime(2026, 9, 10, 8, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_008", new DateTime(2026, 9, 10, 8, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 13.12m, new DateTime(2026, 9, 14, 10, 1, 0, 0, DateTimeKind.Utc), 9, new DateTime(2026, 9, 14, 10, 2, 0, 0, DateTimeKind.Utc), "pi_seed_order_009", new DateTime(2026, 9, 14, 10, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 9.71m, new DateTime(2026, 9, 18, 12, 1, 0, 0, DateTimeKind.Utc), 10, new DateTime(2026, 9, 18, 12, 2, 0, 0, DateTimeKind.Utc), 2, "pi_seed_order_010", new DateTime(2026, 9, 18, 12, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 7.18m, new DateTime(2026, 9, 22, 9, 1, 0, 0, DateTimeKind.Utc), 11, new DateTime(2026, 9, 22, 9, 2, 0, 0, DateTimeKind.Utc), 3, "pi_seed_order_011", new DateTime(2026, 9, 22, 9, 2, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Excellent from start to finish.", new DateTime(2026, 9, 9, 8, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Quick pick-up and a very polite driver.", new DateTime(2026, 9, 11, 9, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Smooth ride, the car was spotless.", new DateTime(2026, 9, 13, 10, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "The driver took a long detour without saying why.", new DateTime(2026, 9, 19, 9, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Arrived on time and drove carefully.", new DateTime(2026, 9, 10, 8, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Could not have been better.", new DateTime(2026, 9, 14, 10, 55, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 24, 14, 55, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 11, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 11, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 11, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 13, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 12, 45, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 12, 45, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 10, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 11, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 9, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 15, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 9, 15, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 11, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 15, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 11, 15, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 15, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 13, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 13, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 17, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 17, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 19, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 19, 17, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 21, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 21, 17, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 10, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 14, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 14, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 18, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 18, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 22, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 22, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 9, 24, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 16, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 24, 16, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "StartedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 6, 45, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 6, 45, 0, 0, DateTimeKind.Utc) });


            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 13);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), 1, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc), 1, 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc), 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[] { new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), 3, 1, 1, 4, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc), 3, 2, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), 4, 1, 1, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc), 4, 2, 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), 5, 1, 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId" },
                values: new object[] { new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc), 5, 2, 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc), 2, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), 7, 2, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc), 7, 2, 4 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), 8, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc), 8, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), 9, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc), 9, 2, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), 10, 1, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc), 10, 2 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId" },
                values: new object[] { new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), 11, 1 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc), 11, 2, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), 12, 4, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc), 12, 4, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "CreatedAt", "OrderId", "Value" },
                values: new object[] { new DateTime(2026, 6, 11, 10, 35, 0, 0, DateTimeKind.Utc), 1, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 6, 18, 10, 35, 0, 0, DateTimeKind.Utc), 3, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 6, 25, 10, 35, 0, 0, DateTimeKind.Utc), 3, 3, 4m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "Value" },
                values: new object[] { new DateTime(2026, 6, 14, 10, 35, 0, 0, DateTimeKind.Utc), 6, 2, 2m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "CreatedAt", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 6, 12, 10, 35, 0, 0, DateTimeKind.Utc), 3, 4m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 6, 20, 10, 35, 0, 0, DateTimeKind.Utc), 9, 3, 5m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "CreatedAt", "OrderId", "TaxiCompanyId", "Value" },
                values: new object[] { new DateTime(2026, 6, 28, 10, 35, 0, 0, DateTimeKind.Utc), 12, 4, 3m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "CreatedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), 13 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId" },
                values: new object[] { new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 14, 1, 3, 9 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "CreatedAt", "OrderId", "SignalTypeId", "Value" },
                values: new object[] { new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), 15, 1, 1m });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "CreatedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), 16 });

            migrationBuilder.UpdateData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[] { 37, new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), 18, 1, 3, 9, 1m });

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "ReadAt" },
                values: new object[] { new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "ReadAt" },
                values: new object[] { new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 11, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 11, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 18, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 18, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 8,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 9,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 10,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 25, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 11,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 25, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 12,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 13,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 14,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 2, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 15,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 2, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 16,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 17,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 18,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 9, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 19,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 9, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 20,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 21,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 22,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 14, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 23,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 14, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 24,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 25,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 26,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 16, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 27,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 16, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 28,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 29,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 30,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 12, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 31,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 12, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 32,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 33,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 34,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 20, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 35,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 20, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 36,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 37,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 38,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 4, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 39,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 4, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 40,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 41,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 42,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 11, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 43,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 11, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 44,
                column: "ChangedAt",
                value: new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 45,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 46,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 28, 9, 5, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 47,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 28, 9, 12, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 48,
                column: "ChangedAt",
                value: new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ChangedAt", "OrderId" },
                values: new object[] { new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), 13 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 9, null, 14, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), 4, 15 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc), 3, 15 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), 9, null, 16, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 8, 5, 18, 9, 0, 0, DateTimeKind.Utc), 8, 1, 16, "Accepted by driver", 2 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc), 8, 2, 16, "Passenger picked up", 3 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId" },
                values: new object[] { new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), 4, 17 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "ChangedAt", "ChangedByUserId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc), 4, 17, "Passenger found another ride before a driver accepted.", 5 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), 9, null, 18, "Order placed", 1 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 7, 22, 9, 19, 0, 0, DateTimeKind.Utc), 8, 1, 18, "Accepted by driver", 2 });

            migrationBuilder.UpdateData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[] { new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc), 8, 2, 18, "Driver could not reach the pick-up point because of a closed road.", 5 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 2, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 9, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 16, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 4, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 11, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "AcceptedAt", "CompletedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 28, 9, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 12, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 9, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "Fare", "PrepaidAt", "TaxiCompanyId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 7.29m, new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 3, null });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "AcceptedAt", "CarId", "CreatedAt", "DriverId", "PassengerId", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc), 1, new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), 1, 4, new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "AcceptedAt", "CreatedAt", "PickedUpAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 5, 18, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CancelledAt", "CreatedAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "AcceptedAt", "CancelledAt", "CreatedAt", "PrepaidAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 22, 9, 19, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CityId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "IsPrepaid", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "PrepaidAt", "TaxiCompanyId", "UpdatedAt" },
                values: new object[] { 13, null, null, null, null, 1, null, new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), null, "Kampus Sveucilista", 43.341900000000003, 17.804300000000001, 5.50m, null, 9.10m, false, true, 1, 4, null, "Rondo 1", 43.3489, 17.799499999999998, new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), 1, null });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 10, "Waiting time was charged in error and returned to the passenger.", 2.18m, new DateTime(2026, 7, 12, 8, 20, 0, 0, DateTimeKind.Utc), "re_seed_010" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 11, "The ride was billed twice, so the duplicate charge was returned in full.", 9.01m, new DateTime(2026, 6, 29, 8, 20, 0, 0, DateTimeKind.Utc), "re_seed_011" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 3, "The passenger was charged for a longer route than the one driven.", 3.58m, new DateTime(2026, 6, 26, 9, 15, 0, 0, DateTimeKind.Utc), "re_seed_003" });

            migrationBuilder.UpdateData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[] { 4, "The ride was cut short by a breakdown, so the full fare was returned.", 7.42m, new DateTime(2026, 7, 3, 8, 40, 0, 0, DateTimeKind.Utc), "re_seed_004" });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 9, 40, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_001", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 9, 40, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_002", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 9, 40, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_003", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 2, 9, 40, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_004", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "IsPaid", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 9, 9, 40, 0, 0, DateTimeKind.Utc), false, null, 1, "pi_seed_005", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 9, 40, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_006", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 7.95m, new DateTime(2026, 6, 12, 9, 40, 0, 0, DateTimeKind.Utc), 8, new DateTime(2026, 6, 12, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_008", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 13.12m, new DateTime(2026, 6, 20, 9, 40, 0, 0, DateTimeKind.Utc), 9, new DateTime(2026, 6, 20, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_009", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 9.71m, new DateTime(2026, 7, 4, 9, 40, 0, 0, DateTimeKind.Utc), 10, new DateTime(2026, 7, 4, 10, 5, 0, 0, DateTimeKind.Utc), "pi_seed_010", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 7.18m, new DateTime(2026, 7, 11, 9, 40, 0, 0, DateTimeKind.Utc), 11, new DateTime(2026, 7, 11, 10, 5, 0, 0, DateTimeKind.Utc), 3, "pi_seed_011", null });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AmountCharged", "CreatedAt", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[] { 9.01m, new DateTime(2026, 6, 28, 9, 40, 0, 0, DateTimeKind.Utc), 12, new DateTime(2026, 6, 28, 10, 5, 0, 0, DateTimeKind.Utc), 4, "pi_seed_012", null });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Quick pickup and a friendly driver.", new DateTime(2026, 6, 11, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Always on time, never had a problem.", new DateTime(2026, 6, 18, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Comfortable ride, arrived a few minutes late.", new DateTime(2026, 6, 25, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Waited far too long for the car to show up.", new DateTime(2026, 6, 14, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Good driver and a clean car.", new DateTime(2026, 6, 12, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Comment", "CreatedAt" },
                values: new object[] { "Excellent from start to finish.", new DateTime(2026, 6, 20, 10, 35, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 28, 10, 35, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 11, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 11, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 12, 45, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 12, 45, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "ShiftBreaks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 11, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 15, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 11, 15, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 15, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 18, 15, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 2, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 25, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 9, 16, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 14, 17, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 16, 17, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 12, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 20, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 4, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 4, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 11, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 11, 15, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "EndedAt", "StartedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 28, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 16, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 28, 16, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "StartedAt" },
                values: new object[] { new DateTime(2026, 8, 6, 6, 45, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 6, 6, 45, 0, 0, DateTimeKind.Utc) });
        }
    }
}
