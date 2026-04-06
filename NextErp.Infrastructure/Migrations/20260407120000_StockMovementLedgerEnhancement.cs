using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockMovementLedgerEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousQuantity",
                table: "StockMovements",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NewQuantity",
                table: "StockMovements",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityChanged",
                table: "StockMovements",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE sm
                SET StockId = s.Id,
                    QuantityChanged = sm.Quantity
                FROM StockMovements sm
                INNER JOIN Stocks s ON s.ProductVariantId = sm.ProductVariantId AND s.BranchId = sm.BranchId
                """);

            migrationBuilder.Sql("DELETE FROM StockMovements WHERE StockId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "StockId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "StockMovements");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockId",
                table: "StockMovements",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                table: "StockMovements",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockId",
                table: "StockMovements");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "StockMovements",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE StockMovements SET Quantity = QuantityChanged");

            migrationBuilder.DropColumn(name: "StockId", table: "StockMovements");
            migrationBuilder.DropColumn(name: "PreviousQuantity", table: "StockMovements");
            migrationBuilder.DropColumn(name: "NewQuantity", table: "StockMovements");
            migrationBuilder.DropColumn(name: "QuantityChanged", table: "StockMovements");
        }
    }
}
