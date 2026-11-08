using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class RefundClientIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientRefundKey",
                table: "PaymentRefunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_ClientRefundKey",
                table: "PaymentRefunds",
                column: "ClientRefundKey",
                unique: true,
                filter: "\"ClientRefundKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentRefunds_ClientRefundKey",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ClientRefundKey",
                table: "PaymentRefunds");
        }
    }
}
