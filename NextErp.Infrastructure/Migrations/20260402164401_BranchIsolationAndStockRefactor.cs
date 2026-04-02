using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BranchIsolationAndStockRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Sales SET BranchId = '00000000-0000-0000-0000-000000000000' WHERE BranchId IS NULL;");
            migrationBuilder.Sql("UPDATE Purchases SET BranchId = '00000000-0000-0000-0000-000000000000' WHERE BranchId IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO Branches (Id, Title, TenantId, Address, IsActive, CreatedAt, UpdatedAt, Metadata)
                VALUES ('00000000-0000-0000-0000-000000000000', 'Default Branch', '00000000-0000-0000-0000-000000000000', NULL, 1, SYSUTCDATETIME(), NULL, '{}');
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_ProductVariants_Id",
                table: "Stocks");

            migrationBuilder.Sql("EXEC sp_rename 'Stocks', 'Stocks_Old';");
            migrationBuilder.Sql(
                """
                DECLARE @pkName sysname;
                SELECT @pkName = kc.name
                FROM sys.key_constraints kc
                WHERE kc.parent_object_id = OBJECT_ID(N'[Stocks_Old]')
                  AND kc.[type] = 'PK';
                IF @pkName IS NOT NULL
                    EXEC(N'ALTER TABLE [Stocks_Old] DROP CONSTRAINT [' + @pkName + ']');

                DECLARE @dropIndexesSql nvarchar(max) = N'';
                SELECT @dropIndexesSql = @dropIndexesSql +
                    N'DROP INDEX [' + i.name + N'] ON [Stocks_Old];'
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(N'[Stocks_Old]')
                  AND i.is_primary_key = 0
                  AND i.is_unique_constraint = 0
                  AND i.name IS NOT NULL;
                IF LEN(@dropIndexesSql) > 0
                    EXEC sp_executesql @dropIndexesSql;
                """);

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO Stocks (Id, Title, ProductVariantId, AvailableQuantity, CreatedAt, UpdatedAt, TenantId, BranchId)
                SELECT NEWID(), s.Title, s.Id, s.AvailableQuantity, s.CreatedAt, s.UpdatedAt, s.TenantId,
                       COALESCE(s.BranchId, '00000000-0000-0000-0000-000000000000')
                FROM Stocks_Old s;
                """);

            migrationBuilder.DropTable(name: "Stocks_Old");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductVariantId",
                table: "Stocks",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductVariantId_BranchId",
                table: "Stocks",
                columns: new[] { "ProductVariantId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_IsActive",
                table: "Branches",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Title",
                table: "Branches",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_ProductVariants_ProductVariantId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductVariantId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductVariantId_BranchId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers");

            migrationBuilder.Sql("EXEC sp_rename 'Stocks', 'Stocks_New';");
            migrationBuilder.Sql(
                """
                DECLARE @pkName sysname;
                SELECT @pkName = kc.name
                FROM sys.key_constraints kc
                WHERE kc.parent_object_id = OBJECT_ID(N'[Stocks_New]')
                  AND kc.[type] = 'PK';
                IF @pkName IS NOT NULL
                    EXEC(N'ALTER TABLE [Stocks_New] DROP CONSTRAINT [' + @pkName + ']');

                DECLARE @dropIndexesSql nvarchar(max) = N'';
                SELECT @dropIndexesSql = @dropIndexesSql +
                    N'DROP INDEX [' + i.name + N'] ON [Stocks_New];'
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(N'[Stocks_New]')
                  AND i.is_primary_key = 0
                  AND i.is_unique_constraint = 0
                  AND i.name IS NOT NULL;
                IF LEN(@dropIndexesSql) > 0
                    EXEC sp_executesql @dropIndexesSql;
                """);

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_ProductVariants_Id",
                        column: x => x.Id,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                ;WITH RankedStocks AS
                (
                    SELECT s.*,
                           ROW_NUMBER() OVER (PARTITION BY s.ProductVariantId ORDER BY s.CreatedAt DESC, s.Id) AS RowNum
                    FROM Stocks_New s
                )
                INSERT INTO Stocks (Id, Title, AvailableQuantity, CreatedAt, UpdatedAt, TenantId, BranchId)
                SELECT ProductVariantId, Title, AvailableQuantity, CreatedAt, UpdatedAt, TenantId,
                       NULLIF(BranchId, '00000000-0000-0000-0000-000000000000')
                FROM RankedStocks
                WHERE RowNum = 1;
                """);

            migrationBuilder.DropTable(name: "Stocks_New");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Stocks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
