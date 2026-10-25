using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSupportSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicketQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Query",
                table: "Tickets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Query",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
