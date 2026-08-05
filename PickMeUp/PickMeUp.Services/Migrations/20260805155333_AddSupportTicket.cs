using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PickMeUp.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmitterId = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SupportTicketStatusId = table.Column<int>(type: "integer", nullable: false),
                    TargetUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTicketStatuses_SupportTicketStatusId",
                        column: x => x.SupportTicketStatusId,
                        principalTable: "SupportTicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_SubmitterId",
                        column: x => x.SubmitterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupportTicketId = table.Column<int>(type: "integer", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketReplies_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketReplies_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SupportTickets",
                columns: new[] { "Id", "Body", "CreatedAt", "DeletedAt", "IsDeleted", "ResolvedAt", "ResolvedByUserId", "Subject", "SubmitterId", "SupportTicketStatusId", "TargetUserId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Twice this week the app returned me to the login screen while my ride was still in progress.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, null, "The app signs me out during a ride", 4, 1, null, null },
                    { 2, "Starting a shift fails with an error even though a car is assigned to me.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, null, "I cannot start my shift", 3, 2, null, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "The driver drove across town for a trip that normally takes ten minutes and the fare was far higher than usual.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Driver took a much longer route", 4, 3, 3, new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "We uploaded a logo from the desktop app but passengers still see an empty placeholder.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, null, "Our company logo does not show in the app", 2, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "SupportTicketReplies",
                columns: new[] { "Id", "AuthorId", "Body", "CreatedAt", "SupportTicketId" },
                values: new object[,]
                {
                    { 1, 1, "Thanks for reporting this. We are checking your car assignment now.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 2, 1, "We have pulled the route for this ride and are reviewing it with the company.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 3, 1, "The difference has been refunded and the company has been notified.", new DateTime(2026, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_AuthorId",
                table: "SupportTicketReplies",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketReplies_SupportTicketId",
                table: "SupportTicketReplies",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ResolvedByUserId",
                table: "SupportTickets",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_SubmitterId",
                table: "SupportTickets",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_SupportTicketStatusId_CreatedAt",
                table: "SupportTickets",
                columns: new[] { "SupportTicketStatusId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TargetUserId",
                table: "SupportTickets",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportTicketReplies");

            migrationBuilder.DropTable(
                name: "SupportTickets");
        }
    }
}
