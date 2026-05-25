using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PurchaseItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBatches_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_Open",
                table: "StockBatches",
                columns: new[] { "ProductVariantId", "BranchId", "ReceivedAt" },
                filter: "[RemainingQuantity] > 0");

            // Backfill: one synthetic opening-balance batch per existing Stock
            // row that has on-hand quantity, sourcing unit cost from Product.Cost.
            // Idempotent — NOT EXISTS guard skips variants that already have
            // a batch, so the migration is safe to re-run during recovery.
            migrationBuilder.Sql(@"
INSERT INTO [StockBatches]
    ([Id], [Title], [ProductVariantId], [BranchId], [TenantId],
     [ReceivedAt], [OriginalQuantity], [RemainingQuantity], [UnitCost],
     [PurchaseItemId], [IsActive], [CreatedAt])
SELECT
    NEWID(),
    'Opening balance',
    s.[ProductVariantId],
    s.[BranchId],
    s.[TenantId],
    ISNULL(s.[CreatedAt], SYSUTCDATETIME()),
    s.[AvailableQuantity],
    s.[AvailableQuantity],
    ISNULL(p.[Cost], 0),
    NULL,
    1,
    SYSUTCDATETIME()
FROM [Stocks] s
INNER JOIN [ProductVariants] pv ON pv.[Id] = s.[ProductVariantId]
INNER JOIN [Products] p ON p.[Id] = pv.[ProductId]
WHERE s.[AvailableQuantity] > 0
  AND NOT EXISTS (
      SELECT 1 FROM [StockBatches] b
      WHERE b.[ProductVariantId] = s.[ProductVariantId]
        AND b.[BranchId] = s.[BranchId]
  );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockBatches");
        }
    }
}
