using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VariantSkuLevelStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Products_ProductId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Products_ProductId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItems_ProductId",
                table: "PurchaseItems");

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "SaleItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "PurchaseItems",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO ProductVariants (Title, Name, ProductId, Sku, Price, Stock, IsActive, CreatedAt, TenantId, BranchId)
                SELECT
                    p.Title,
                    p.Title,
                    p.Id,
                    CASE WHEN NULLIF(LTRIM(RTRIM(p.Code)), N'') IS NULL
                        THEN CONCAT(N'P', CAST(p.Id AS NVARCHAR(20)), N'-DEFAULT')
                        ELSE CONCAT(LTRIM(RTRIM(p.Code)), N'-DEFAULT') END,
                    p.Price,
                    p.Stock,
                    CAST(1 AS bit),
                    GETUTCDATE(),
                    p.TenantId,
                    p.BranchId
                FROM Products p
                WHERE NOT EXISTS (SELECT 1 FROM ProductVariants pv WHERE pv.ProductId = p.Id);
                """);

            migrationBuilder.Sql("""
                UPDATE si SET ProductVariantId = (
                    SELECT MIN(pv.Id) FROM ProductVariants pv WHERE pv.ProductId = si.ProductId
                )
                FROM SaleItems si;
                """);

            migrationBuilder.Sql("""
                UPDATE pi SET ProductVariantId = (
                    SELECT MIN(pv.Id) FROM ProductVariants pv WHERE pv.ProductId = pi.ProductId
                )
                FROM PurchaseItems pi;
                """);

            migrationBuilder.Sql("DELETE FROM SaleItems WHERE ProductVariantId IS NULL");
            migrationBuilder.Sql("DELETE FROM PurchaseItems WHERE ProductVariantId IS NULL");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "SaleItems");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantId",
                table: "SaleItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "PurchaseItems");

            migrationBuilder.AlterColumn<int>(
                name: "ProductVariantId",
                table: "PurchaseItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductVariantId",
                table: "SaleItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductVariantId",
                table: "PurchaseItems",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_ProductVariants_ProductVariantId",
                table: "SaleItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_ProductVariants_ProductVariantId",
                table: "PurchaseItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "StockNewId",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE s SET StockNewId = (
                    SELECT MIN(pv.Id) FROM ProductVariants pv WHERE pv.ProductId = s.ProductId
                )
                FROM Stocks s;
                """);

            migrationBuilder.Sql("DELETE FROM Stocks WHERE StockNewId IS NULL");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Stocks");

            migrationBuilder.RenameColumn(
                name: "StockNewId",
                table: "Stocks",
                newName: "Id");

            // Renamed column keeps nullability; PK requires NOT NULL.
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Stocks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_ProductVariants_Id",
                table: "Stocks",
                column: "Id",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                INSERT INTO Stocks (Id, Title, AvailableQuantity, TenantId, BranchId, CreatedAt)
                SELECT pv.Id,
                    CONCAT(N'Stock-', pv.Sku),
                    CAST(CASE WHEN pv.Stock < 0 THEN 0 ELSE pv.Stock END AS DECIMAL(18,2)),
                    pv.TenantId,
                    pv.BranchId,
                    GETUTCDATE()
                FROM ProductVariants pv
                WHERE NOT EXISTS (SELECT 1 FROM Stocks s WHERE s.Id = pv.Id);
                """);

            migrationBuilder.Sql("""
                UPDATE pv
                SET Stock = CAST(ROUND(s.AvailableQuantity, 0) AS INT)
                FROM ProductVariants pv
                INNER JOIN Stocks s ON s.Id = pv.Id;
                """);

            migrationBuilder.Sql("""
                UPDATE p
                SET Stock = ISNULL(v.SumStock, 0)
                FROM Products p
                LEFT JOIN (
                    SELECT ProductId, SUM(Stock) AS SumStock
                    FROM ProductVariants
                    GROUP BY ProductId
                ) v ON v.ProductId = p.Id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Reverting variant-level stock is not supported. Restore from backup if required.");
        }
    }
}
