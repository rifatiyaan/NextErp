using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.API.Migrations
{
    /// <inheritdoc />
    public partial class FixModuleIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints if it exists (check name or just try drop)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Modules_Modules_ParentId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Modules]'))
                    ALTER TABLE [dbo].[Modules] DROP CONSTRAINT [FK_Modules_Modules_ParentId];
            ");

            // Drop indexes before renaming to avoid naming conflicts if kept
            migrationBuilder.DropIndex(name: "IX_Modules_ParentId", table: "Modules");
            migrationBuilder.DropIndex(name: "IX_Modules_Type", table: "Modules");
            migrationBuilder.DropIndex(name: "IX_Modules_TenantId_IsActive", table: "Modules");

            // Rename existing table
            migrationBuilder.RenameTable(name: "Modules", newName: "Modules_Old");

            // Create new table with IDENTITY
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

            // Copy data
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Modules ON;
                INSERT INTO Modules (Id, Title, Icon, Url, ParentId, Type, Description, Version, IsInstalled, IsEnabled, InstalledAt, [Order], IsActive, IsExternal, CreatedAt, UpdatedAt, TenantId, BranchId, Metadata)
                SELECT Id, Title, Icon, Url, ParentId, Type, Description, Version, IsInstalled, IsEnabled, InstalledAt, [Order], IsActive, IsExternal, CreatedAt, UpdatedAt, TenantId, BranchId, Metadata
                FROM Modules_Old
                WHERE Id != 0; -- Exclude invalid 0 IDs if any
                SET IDENTITY_INSERT Modules OFF;
            ");
            
            // Reseed Identity
            migrationBuilder.Sql(@"
                DECLARE @maxId int;
                SELECT @maxId = MAX(Id) FROM Modules;
                IF @maxId IS NULL SET @maxId = 0;
                DBCC CHECKIDENT ('Modules', RESEED, @maxId);
            ");

            // Drop old table
            migrationBuilder.DropTable(name: "Modules_Old");

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
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7563));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7604));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7606));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7607));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7609));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7611));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7613));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7615));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7616));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7618));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7825));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7830));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7833));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7835));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7837));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7839));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7841));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7844));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7846));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 6, 19, 8, 59, 416, DateTimeKind.Utc).AddTicks(7848));
        }
    }
}
