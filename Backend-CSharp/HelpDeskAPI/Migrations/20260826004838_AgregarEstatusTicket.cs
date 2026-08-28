using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDeskAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstatusTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estatus",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estatus",
                table: "Tickets");
        }
    }
}
