using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropReturnsTitleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "SaleReturns");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SaleReturnItems");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PurchaseReturnItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SaleReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SaleReturnItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PurchaseReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PurchaseReturnItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
