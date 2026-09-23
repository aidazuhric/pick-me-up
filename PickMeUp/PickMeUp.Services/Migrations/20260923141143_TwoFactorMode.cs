using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class TwoFactorMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorConfirmedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TwoFactorMode",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"TwoFactorMode\" = 1 WHERE \"TwoFactorEnabled\" = true;");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 2 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 1 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "TwoFactorConfirmedAt", "TwoFactorMode" },
                values: new object[] { null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"TwoFactorEnabled\" = true WHERE \"TwoFactorMode\" <> 0;");

            migrationBuilder.DropColumn(
                name: "TwoFactorConfirmedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorMode",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 9,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 10,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 11,
                column: "TwoFactorEnabled",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 12,
                column: "TwoFactorEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 13,
                column: "TwoFactorEnabled",
                value: false);
        }
    }
}
