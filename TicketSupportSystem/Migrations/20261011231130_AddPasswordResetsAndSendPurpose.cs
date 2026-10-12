using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSupportSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetsAndSendPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailSends_Email",
                table: "EmailSends");

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "EmailSends",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PendingResets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingResets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingResets_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSends_Email_Purpose",
                table: "EmailSends",
                columns: new[] { "Email", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingResets_TokenHash",
                table: "PendingResets",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingResets_UserID",
                table: "PendingResets",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingResets");

            migrationBuilder.DropIndex(
                name: "IX_EmailSends_Email_Purpose",
                table: "EmailSends");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EmailSends");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSends_Email",
                table: "EmailSends",
                column: "Email",
                unique: true);
        }
    }
}
