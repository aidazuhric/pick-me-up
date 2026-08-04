using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "IsActive", "Name", "PostalCode" },
                values: new object[,]
                {
                    { 1, true, "Mostar", "88000" },
                    { 2, true, "Sarajevo", "71000" },
                    { 3, true, "Banja Luka", "78000" },
                    { 4, true, "Tuzla", "75000" },
                    { 5, true, "Zenica", "72000" }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Waiting for a driver to accept", true, "Pending", 1 },
                    { 2, "A driver accepted and is on the way", true, "Confirmed", 2 },
                    { 3, "Passenger picked up, ride underway", true, "InProgress", 3 },
                    { 4, "Ride finished, payment follows", true, "Completed", 4 },
                    { 5, "Cancelled with a mandatory reason", true, "Cancelled", 5 }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "No successful charge recorded", true, "Unpaid", 1 },
                    { 2, "Confirmed by the payment provider webhook", true, "Paid", 2 },
                    { 3, "Part of the charged amount returned", true, "PartiallyRefunded", 3 },
                    { 4, "Full charged amount returned", true, "Refunded", 4 }
                });

            migrationBuilder.InsertData(
                table: "SupportTicketStatuses",
                columns: new[] { "Id", "Description", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Submitted and awaiting review", true, "Open", 1 },
                    { 2, "Being handled by an administrator", true, "InProgress", 2 },
                    { 3, "Closed with a reply", true, "Resolved", 3 }
                });

            migrationBuilder.InsertData(
                table: "VehicleCategories",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Everyday four-seat saloon", true, "Standard" },
                    { 2, "Higher class vehicle with extra legroom", true, "Comfort" },
                    { 3, "Six to eight seats for groups and luggage", true, "Van" },
                    { 4, "Executive vehicle for business rides", true, "Premium" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatuses_Name",
                table: "OrderStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_Name",
                table: "PaymentStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketStatuses_Name",
                table: "SupportTicketStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCategories_Name",
                table: "VehicleCategories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "SupportTicketStatuses");

            migrationBuilder.DropTable(
                name: "VehicleCategories");
        }
    }
}
