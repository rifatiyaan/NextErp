using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionsAndDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sale + SaleItem discount fields
            migrationBuilder.AddColumn<int>(
                name: "DiscountSource",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoicePromotionId",
                table: "Sales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountSource",
                table: "SaleItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionId",
                table: "SaleItems",
                type: "uniqueidentifier",
                nullable: true);

            // Purchase + PurchaseItem discount fields
            migrationBuilder.AddColumn<int>(
                name: "DiscountSource",
                table: "Purchases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountSource",
                table: "PurchaseItems",
                type: "int",
                nullable: true);

            // Party.MembershipTier
            migrationBuilder.AddColumn<string>(
                name: "MembershipTier",
                table: "Parties",
                type: "nvarchar(max)",
                nullable: true);

            // Promotions table with owned PromotionConfig JSON
            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Stackable = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Config = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_IsActive",
                table: "Promotions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_StartDate_EndDate",
                table: "Promotions",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Type",
                table: "Promotions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropColumn(name: "DiscountSource", table: "Sales");
            migrationBuilder.DropColumn(name: "InvoicePromotionId", table: "Sales");
            migrationBuilder.DropColumn(name: "Discount", table: "SaleItems");
            migrationBuilder.DropColumn(name: "DiscountSource", table: "SaleItems");
            migrationBuilder.DropColumn(name: "PromotionId", table: "SaleItems");
            migrationBuilder.DropColumn(name: "DiscountSource", table: "Purchases");
            migrationBuilder.DropColumn(name: "Discount", table: "PurchaseItems");
            migrationBuilder.DropColumn(name: "DiscountSource", table: "PurchaseItems");
            migrationBuilder.DropColumn(name: "MembershipTier", table: "Parties");
        }
    }
}
