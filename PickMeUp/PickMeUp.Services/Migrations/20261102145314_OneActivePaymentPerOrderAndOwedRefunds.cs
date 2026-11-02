using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class OneActivePaymentPerOrderAndOwedRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "IntentAttempt",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RefundRequired",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[] { 5, "The payment attempt never produced a charge", true, "Failed", 5 });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "IntentAttempt", "RefundRequired" },
                values: new object[] { 1, false });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true,
                filter: "\"PaymentStatusId\" NOT IN (4, 5) AND NOT \"IsDeleted\"");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_StripeRefundId",
                table: "PaymentRefunds",
                column: "StripeRefundId",
                unique: true,
                filter: "\"StripeRefundId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_StripeRefundId",
                table: "PaymentRefunds");

            migrationBuilder.DeleteData(
                table: "PaymentStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "IntentAttempt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundRequired",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");
        }
    }
}
