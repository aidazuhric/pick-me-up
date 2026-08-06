using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedPaymentsAndOrderVariety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AcceptedAt", "CancellationReason", "CancelledAt", "CarId", "CompletedAt", "CreatedAt", "DeletedAt", "DestinationAddress", "DestinationLat", "DestinationLng", "DistanceKm", "DriverId", "Fare", "IsDeleted", "OrderStatusId", "PassengerId", "PickedUpAt", "PickupAddress", "PickupLat", "PickupLng", "TaxiCompanyId", "UpdatedAt" },
                values: new object[,]
                {
                    { 13, null, null, null, null, null, new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), null, "Kampus Sveucilista", 43.341900000000003, 17.804300000000001, 5.50m, null, null, false, 1, 4, null, "Rondo 1", 43.3489, 17.799499999999998, 1, null },
                    { 14, null, null, null, null, null, new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), null, "Marijin Dvor", 43.856000000000002, 18.402999999999999, 3.90m, null, null, false, 1, 9, null, "Ferhadija 10", 43.859000000000002, 18.420999999999999, 3, null },
                    { 15, new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc), null, null, 1, null, new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), null, "Stari Most", 43.337400000000002, 17.815100000000001, 6.70m, 1, null, false, 2, 4, null, "Splitska 5", 43.335000000000001, 17.802099999999999, 1, new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc) },
                    { 16, new DateTime(2026, 8, 5, 18, 9, 0, 0, DateTimeKind.Utc), null, null, 5, null, new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), null, "Ilidza", 43.828000000000003, 18.309999999999999, 8.20m, 3, null, false, 3, 9, new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc), "Bascarsija", 43.859499999999997, 18.431000000000001, 3, new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc) },
                    { 17, null, "Passenger found another ride before a driver accepted.", new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), null, "Zeljeznicka stanica", 43.347999999999999, 17.808499999999999, 4.40m, null, null, false, 5, 4, null, "Marsala Tita 40", 43.3401, 17.813500000000001, 1, new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc) },
                    { 18, new DateTime(2026, 7, 22, 9, 19, 0, 0, DateTimeKind.Utc), "Driver could not reach the pick-up point because of a closed road.", new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc), 5, null, new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), null, "Vogosca", 43.899999999999999, 18.343, 6.10m, 3, null, false, 5, 9, null, "Grbavica", 43.847999999999999, 18.395, 3, new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "AmountCharged", "CreatedAt", "Currency", "DeletedAt", "IsDeleted", "IsPaid", "OrderId", "PaidAt", "PaymentStatusId", "StripePaymentIntentId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 6.34m, new DateTime(2026, 6, 11, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 1, new DateTime(2026, 6, 11, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_001", null },
                    { 2, 9.22m, new DateTime(2026, 6, 18, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 2, new DateTime(2026, 6, 18, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_002", null },
                    { 3, 12.58m, new DateTime(2026, 6, 25, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 3, new DateTime(2026, 6, 25, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_003", null },
                    { 4, 7.42m, new DateTime(2026, 7, 2, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 4, new DateTime(2026, 7, 2, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_004", null },
                    { 5, 10.78m, new DateTime(2026, 7, 9, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, false, 5, null, 1, "pi_seed_005", null },
                    { 6, 8.75m, new DateTime(2026, 6, 14, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 6, new DateTime(2026, 6, 14, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_006", null },
                    { 7, 7.95m, new DateTime(2026, 6, 12, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 8, new DateTime(2026, 6, 12, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_008", null },
                    { 8, 13.12m, new DateTime(2026, 6, 20, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 9, new DateTime(2026, 6, 20, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_009", null },
                    { 9, 9.71m, new DateTime(2026, 7, 4, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 10, new DateTime(2026, 7, 4, 10, 5, 0, 0, DateTimeKind.Utc), 2, "pi_seed_010", null },
                    { 10, 7.18m, new DateTime(2026, 7, 11, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 11, new DateTime(2026, 7, 11, 10, 5, 0, 0, DateTimeKind.Utc), 3, "pi_seed_011", null },
                    { 11, 9.01m, new DateTime(2026, 6, 28, 9, 40, 0, 0, DateTimeKind.Utc), "BAM", null, false, true, 12, new DateTime(2026, 6, 28, 10, 5, 0, 0, DateTimeKind.Utc), 4, "pi_seed_012", null }
                });

            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[,]
                {
                    { 32, new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), 13, 1, 1, 4, 1m },
                    { 33, new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 14, 1, 3, 9, 1m },
                    { 34, new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), 15, 1, 1, 4, 1m },
                    { 35, new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), 16, 1, 3, 9, 1m },
                    { 36, new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), 17, 1, 1, 4, 1m },
                    { 37, new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), 18, 1, 3, 9, 1m }
                });

            migrationBuilder.InsertData(
                table: "OrderStatusHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedByUserId", "FromStatusId", "OrderId", "Reason", "ToStatusId" },
                values: new object[,]
                {
                    { 49, new DateTime(2026, 8, 3, 8, 30, 0, 0, DateTimeKind.Utc), 4, null, 13, "Order placed", 1 },
                    { 50, new DateTime(2026, 8, 4, 17, 10, 0, 0, DateTimeKind.Utc), 9, null, 14, "Order placed", 1 },
                    { 51, new DateTime(2026, 8, 5, 7, 45, 0, 0, DateTimeKind.Utc), 4, null, 15, "Order placed", 1 },
                    { 52, new DateTime(2026, 8, 5, 7, 50, 0, 0, DateTimeKind.Utc), 3, 1, 15, "Accepted by driver", 2 },
                    { 53, new DateTime(2026, 8, 5, 18, 5, 0, 0, DateTimeKind.Utc), 9, null, 16, "Order placed", 1 },
                    { 54, new DateTime(2026, 8, 5, 18, 9, 0, 0, DateTimeKind.Utc), 8, 1, 16, "Accepted by driver", 2 },
                    { 55, new DateTime(2026, 8, 5, 18, 16, 0, 0, DateTimeKind.Utc), 8, 2, 16, "Passenger picked up", 3 },
                    { 56, new DateTime(2026, 7, 20, 12, 0, 0, 0, DateTimeKind.Utc), 4, null, 17, "Order placed", 1 },
                    { 57, new DateTime(2026, 7, 20, 12, 6, 0, 0, DateTimeKind.Utc), 4, 1, 17, "Passenger found another ride before a driver accepted.", 5 },
                    { 58, new DateTime(2026, 7, 22, 9, 15, 0, 0, DateTimeKind.Utc), 9, null, 18, "Order placed", 1 },
                    { 59, new DateTime(2026, 7, 22, 9, 19, 0, 0, DateTimeKind.Utc), 8, 1, 18, "Accepted by driver", 2 },
                    { 60, new DateTime(2026, 7, 22, 9, 31, 0, 0, DateTimeKind.Utc), 8, 2, 18, "Driver could not reach the pick-up point because of a closed road.", 5 }
                });

            migrationBuilder.InsertData(
                table: "PaymentRefunds",
                columns: new[] { "Id", "PaymentId", "Reason", "RefundedAmount", "RefundedAt", "StripeRefundId" },
                values: new object[,]
                {
                    { 1, 10, "Waiting time was charged in error and returned to the passenger.", 2.18m, new DateTime(2026, 7, 12, 8, 20, 0, 0, DateTimeKind.Utc), "re_seed_010" },
                    { 2, 11, "The ride was billed twice, so the duplicate charge was returned in full.", 9.01m, new DateTime(2026, 6, 29, 8, 20, 0, 0, DateTimeKind.Utc), "re_seed_011" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "OrderStatusHistories",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentRefunds",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
