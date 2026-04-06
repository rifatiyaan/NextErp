using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductVariantStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "ProductVariants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
