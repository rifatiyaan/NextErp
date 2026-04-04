using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockAndStockMovementIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stocks",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StockMovements",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StockMovements");
        }
    }
}
