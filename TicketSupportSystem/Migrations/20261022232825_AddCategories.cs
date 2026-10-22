using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TicketSupportSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HandledByLevel = table.Column<int>(type: "integer", nullable: false),
                    DefaultPriority = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "DefaultPriority", "HandledByLevel", "IsEnabled", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, 0, true, "General Question", 1 },
                    { 2, 1, 0, true, "Billing", 2 },
                    { 3, 1, 1, true, "Account", 3 },
                    { 4, 1, 1, true, "Product", 4 },
                    { 5, 0, 0, true, "Feedback / Complaint", 6 },
                    { 6, 1, 0, true, "Sales", 5 }
                });
            migrationBuilder.Sql("UPDATE \"Tickets\" SET \"CategoryID\" = \"Query\" + 1;");
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CategoryID",
                table: "Tickets",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Categories_CategoryID",
                table: "Tickets",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Categories_CategoryID",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CategoryID",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Tickets");
        }
    }
}
