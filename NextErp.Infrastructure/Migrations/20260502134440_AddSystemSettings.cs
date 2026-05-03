using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PresetAccentTheme = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CustomPrimary = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CustomSecondary = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CustomSidebarBackground = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CustomSidebarForeground = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    NavigationPlacement = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "sidebar"),
                    Radius = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "md"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyLogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_TenantId",
                table: "SystemSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
