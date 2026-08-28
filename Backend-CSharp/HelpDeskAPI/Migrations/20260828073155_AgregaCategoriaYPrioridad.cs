using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCategoriaYPrioridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrioridadId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PrioridadId",
                table: "Tickets");
        }
    }
}
