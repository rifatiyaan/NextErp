using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductTestString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conditional drop — column existed in the model but was
            // already removed from the DB on some environments. Plain
            // DropColumn would fail on those; the IF-guard makes the
            // migration idempotent across drifted databases.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE Name = N'TestString'
                      AND Object_ID = Object_ID(N'[dbo].[Products]')
                )
                BEGIN
                    ALTER TABLE [dbo].[Products] DROP COLUMN [TestString];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TestString",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
