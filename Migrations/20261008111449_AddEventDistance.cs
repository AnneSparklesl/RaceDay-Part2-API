using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_proj.Migrations
{
    /// <inheritdoc />
    public partial class AddEventDistance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "distance_km",
                table: "Event",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "distance_km",
                table: "Event");
        }
    }
}
