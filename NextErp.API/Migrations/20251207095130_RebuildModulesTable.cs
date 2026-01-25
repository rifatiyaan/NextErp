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
            // This migration was created to rebuild the Modules table after manual deletion.
            // However, when running migrations from scratch, the table already exists from
            // the MergeModuleAndMenuItem migration. So we only create it if it doesn't exist.
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Modules', 'U') IS NULL
                BEGIN
                    CREATE TABLE [Modules] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Title] nvarchar(100) NOT NULL,
                        [Icon] nvarchar(50) NULL,
                        [Url] nvarchar(500) NULL,
                        [ParentId] int NULL,
                        [Type] int NOT NULL,
                        [Description] nvarchar(1000) NULL,
                        [Version] nvarchar(20) NULL,
                        [IsInstalled] bit NOT NULL,
                        [IsEnabled] bit NOT NULL,
                        [InstalledAt] datetime2 NULL,
                        [Order] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        [IsExternal] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [BranchId] uniqueidentifier NULL,
                        [Metadata] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Modules] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Modules_Modules_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Modules] ([Id]) ON DELETE NO ACTION
                    );
                END
            ");

            // Ensure indexes exist (create if they don't)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Modules_ParentId' AND object_id = OBJECT_ID('Modules'))
                BEGIN
                    CREATE INDEX [IX_Modules_ParentId] ON [Modules] ([ParentId]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Modules_Type' AND object_id = OBJECT_ID('Modules'))
                BEGIN
                    CREATE INDEX [IX_Modules_Type] ON [Modules] ([Type]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Modules_TenantId_IsActive' AND object_id = OBJECT_ID('Modules'))
                BEGIN
                    CREATE INDEX [IX_Modules_TenantId_IsActive] ON [Modules] ([TenantId], [IsActive]);
                END
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Modules");
        }
    }
}
