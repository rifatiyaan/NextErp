using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BranchScopedAndStockLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.BranchId = b.Id
                FROM Products p
                CROSS APPLY (SELECT TOP (1) Id FROM Branches ORDER BY CreatedAt) AS b
                WHERE p.BranchId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE Products
                SET BranchId = '00000000-0000-0000-0000-000000000001'
                WHERE BranchId IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid?),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BranchId",
                table: "StockMovements",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductVariantId_BranchId_CreatedAt",
                table: "StockMovements",
                columns: new[] { "ProductVariantId", "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReferenceId",
                table: "StockMovements",
                column: "ReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: false);
        }
    }
}
