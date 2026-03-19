using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    public partial class FixStockIdConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint first
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks");

            // Drop the primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks");

            // Drop the unique index on ProductId
            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks");

            // Create a temporary column to hold the data
            migrationBuilder.AddColumn<int>(
                name: "IdNew",
                table: "Stocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Copy ProductId values to the new column
            migrationBuilder.Sql("UPDATE Stocks SET IdNew = ProductId");

            // Drop the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Stocks");

            // Rename the new column to Id
            migrationBuilder.RenameColumn(
                name: "IdNew",
                table: "Stocks",
                newName: "Id");

            // Recreate the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks",
                column: "Id");

            // Recreate the unique index on ProductId
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId",
                unique: true);

            // Recreate the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks");

            // Drop the primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks");

            // Drop the unique index
            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks");

            // Create a temporary column with IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "IdOld",
                table: "Stocks",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            // Copy Id values to the new column (this will generate new IDs)
            migrationBuilder.Sql("UPDATE Stocks SET IdOld = Id");

            // Drop the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Stocks");

            // Rename the new column to Id
            migrationBuilder.RenameColumn(
                name: "IdOld",
                table: "Stocks",
                newName: "Id");

            // Recreate the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Stocks",
                table: "Stocks",
                column: "Id");

            // Recreate the unique index
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId",
                unique: true);

            // Recreate the foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Products_ProductId",
                table: "Stocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
