using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.API.Migrations
{
    /// <inheritdoc />
    public partial class MergeModuleAndMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Save existing Module data to temp table (if any exist)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('TempModules', 'U') IS NOT NULL DROP TABLE TempModules;
                
                IF OBJECT_ID('Modules', 'U') IS NOT NULL
                BEGIN
                    SELECT 
                        CAST(Id AS NVARCHAR(50)) AS OldId,
                        Title,
                        Description,
                        Version,
                        IsInstalled,
                        IsEnabled,
                        InstalledAt,
                        Metadata,
                        TenantId,
                        CreatedAt
                    INTO TempModules
                    FROM Modules;
                END
            ");

            // Step 2: Drop foreign key from MenuItems to Modules (if exists)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MenuItems_Modules_ModuleId')
                BEGIN
                    ALTER TABLE MenuItems DROP CONSTRAINT FK_MenuItems_Modules_ModuleId;
                END
            ");

            // Step 3: Drop old Modules table
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Modules', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE Modules;
                END
            ");

            // Step 4: Rename MenuItems to Modules
            migrationBuilder.RenameTable(
                name: "MenuItems",
                newName: "Modules");

            // Step 5: Rename existing index
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MenuItems_ParentId')
                BEGIN
                    EXEC sp_rename N'dbo.Modules.IX_MenuItems_ParentId', N'IX_Modules_ParentId', N'INDEX';
                END
            ");

            // Step 6: Drop ModuleId index before dropping the column
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MenuItems_ModuleId')
                BEGIN
                    DROP INDEX IX_MenuItems_ModuleId ON Modules;
                END
            ");

            // Step 7: Drop ModuleId column (no longer needed)
            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Modules");

            // Step 8: Add new columns for merged entity
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Modules",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Modules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInstalled",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstalledAt",
                table: "Modules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Modules",
                type: "int",
                nullable: false,
                defaultValue: 2); // Default to Link

            // Step 8: Update existing records to have Type = Link (2)
            migrationBuilder.Sql("UPDATE Modules SET Type = 2 WHERE Type = 0 OR Type IS NULL;");

            // Step 9: Migrate old Module data back as Type = Module (1)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('TempModules', 'U') IS NOT NULL
                BEGIN
                    INSERT INTO Modules (Title, Description, Version, IsInstalled, IsEnabled, 
                                        InstalledAt, Metadata, TenantId, CreatedAt, Type, 
                                        IsActive, [Order], Icon, Url, IsExternal, ParentId, BranchId, UpdatedAt)
                    SELECT 
                        Title,
                        Description,
                        Version,
                        IsInstalled,
                        IsEnabled,
                        InstalledAt,
                        Metadata,
                        TenantId,
                        CreatedAt,
                        1 AS Type, -- Module type
                        1 AS IsActive,
                        0 AS [Order],
                        NULL AS Icon,
                        NULL AS Url,
                        0 AS IsExternal,
                        NULL AS ParentId,
                        NULL AS BranchId,
                        NULL AS UpdatedAt
                    FROM TempModules;
                    
                    DROP TABLE TempModules;
                END
            ");

            // Step 10: Create new indexes
            migrationBuilder.CreateIndex(
                name: "IX_Modules_Type",
                table: "Modules",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_TenantId_IsActive",
                table: "Modules",
                columns: new[] { "TenantId", "IsActive" });

            // Step 11: Add self-referencing foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Modules_ParentId",
                table: "Modules",
                column: "ParentId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Update seed data timestamps
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Modules_ParentId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_ParentId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_TenantId_IsActive",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Type",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Modules");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Modules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Modules",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItems_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8192));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8196));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8198));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8200));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8201));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8203));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8205));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8206));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8208));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8210));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8381));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8387));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8389));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8391));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8393));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8396));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8398));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8402));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 4, 17, 52, 25, 646, DateTimeKind.Utc).AddTicks(8404));

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ModuleId",
                table: "MenuItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");
        }
    }
}
