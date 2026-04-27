using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementReasonAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StockMovements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "StockMovements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "StockMovements");
        }
    }
}
