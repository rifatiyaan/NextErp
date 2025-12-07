using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.API.Migrations
{
    /// <inheritdoc />
    public partial class RebuildModulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create new table with IDENTITY (User deleted old table manually)
            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsInstalled = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Modules_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Recreate Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Modules_ParentId",
                table: "Modules",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Type",
                table: "Modules",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_TenantId_IsActive",
                table: "Modules",
                columns: new[] { "TenantId", "IsActive" });
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Modules");
        }
    }
}
