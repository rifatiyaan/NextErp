using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    public partial class GlobalVariationOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductVariationOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariationOptionId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariationOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariationOptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariationOptions_VariationOptions_VariationOptionId",
                        column: x => x.VariationOptionId,
                        principalTable: "VariationOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ProductVariationOptions (Title, ProductId, VariationOptionId, DisplayOrder, CreatedAt)
                SELECT Name, ProductId, Id, DisplayOrder, CreatedAt FROM VariationOptions
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_VariationOptions_Products_ProductId",
                table: "VariationOptions");

            migrationBuilder.DropIndex(
                name: "IX_VariationOptions_ProductId",
                table: "VariationOptions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "VariationOptions");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariationOptions_ProductId",
                table: "ProductVariationOptions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariationOptions_VariationOptionId",
                table: "ProductVariationOptions",
                column: "VariationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariationOptions_ProductId_VariationOptionId",
                table: "ProductVariationOptions",
                columns: new[] { "ProductId", "VariationOptionId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductVariationOptions");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "VariationOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VariationOptions_ProductId",
                table: "VariationOptions",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_VariationOptions_Products_ProductId",
                table: "VariationOptions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
