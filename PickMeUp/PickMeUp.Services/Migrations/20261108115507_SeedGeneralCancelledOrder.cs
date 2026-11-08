using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class SeedGeneralCancelledOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InteractionHistories",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                column: "TaxiCompanyId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InteractionHistories",
                columns: new[] { "Id", "CreatedAt", "OrderId", "SignalTypeId", "TaxiCompanyId", "UserId", "Value" },
                values: new object[] { 36, new DateTime(2026, 9, 26, 10, 0, 0, 0, DateTimeKind.Utc), 17, 1, 1, 4, 1m });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 17,
                column: "TaxiCompanyId",
                value: 1);
        }
    }
}
