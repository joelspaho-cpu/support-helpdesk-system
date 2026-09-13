using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TicketSupportSystem.Migrations
{
    /// <inheritdoc />
    public partial class DropDepartmentAndAddEngineerReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Departments_DepartmentID",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.RenameColumn(
                name: "DepartmentID",
                table: "Tickets",
                newName: "ReferredToEngineerID");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_DepartmentID",
                table: "Tickets",
                newName: "IX_Tickets_ReferredToEngineerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Staff_ReferredToEngineerID",
                table: "Tickets",
                column: "ReferredToEngineerID",
                principalTable: "Staff",
                principalColumn: "StaffID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Staff_ReferredToEngineerID",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "ReferredToEngineerID",
                table: "Tickets",
                newName: "DepartmentID");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ReferredToEngineerID",
                table: "Tickets",
                newName: "IX_Tickets_DepartmentID");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Departments_DepartmentID",
                table: "Tickets",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID");
        }
    }
}
