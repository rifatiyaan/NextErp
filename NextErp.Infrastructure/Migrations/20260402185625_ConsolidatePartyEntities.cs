using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatePartyEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Step 1: Create new tables first ──────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoyaltyCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PartyType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            // ── Step 2: Copy Customers → Parties (PartyType = 0 = Customer) ──────
            // Customer.Id is already Guid so Sales.CustomerId values remain valid after rename.
            migrationBuilder.Sql(@"
                INSERT INTO [Parties]
                    (Id, Title, Email, Phone, [Address],
                     LoyaltyCode, NationalId, Notes,
                     PartyType, IsActive, CreatedAt, UpdatedAt,
                     TenantId, BranchId)
                SELECT
                    c.Id,
                    c.Title,
                    c.Email,
                    c.Phone,
                    c.[Address],
                    JSON_VALUE(c.Metadata, '$.LoyaltyCode'),
                    JSON_VALUE(c.Metadata, '$.NationalId'),
                    JSON_VALUE(c.Metadata, '$.Notes'),
                    0,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.TenantId,
                    ISNULL(c.BranchId, '00000000-0000-0000-0000-000000000000')
                FROM [Customers] c;
            ");

            // ── Step 3: Copy Suppliers → Parties (PartyType = 1 = Supplier) ──────
            // Supplier.Id is int; new Party.Id is a fresh Guid per row.
            // A temp table captures the mapping so Purchases can be re-pointed.
            migrationBuilder.Sql(@"
                CREATE TABLE #SupplierMap (OldId int PRIMARY KEY, NewId uniqueidentifier NOT NULL);

                INSERT INTO #SupplierMap (OldId, NewId)
                SELECT Id, NEWID() FROM [Suppliers];

                INSERT INTO [Parties]
                    (Id, Title, Email, Phone, [Address], ContactPerson,
                     VatNumber, TaxId, Notes,
                     PartyType, IsActive, CreatedAt, UpdatedAt,
                     TenantId, BranchId)
                SELECT
                    m.NewId,
                    s.Title,
                    s.Email,
                    s.Phone,
                    s.[Address],
                    s.ContactPerson,
                    JSON_VALUE(s.Metadata, '$.VatNumber'),
                    JSON_VALUE(s.Metadata, '$.TaxId'),
                    JSON_VALUE(s.Metadata, '$.Notes'),
                    1,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt,
                    s.TenantId,
                    ISNULL(s.BranchId, '00000000-0000-0000-0000-000000000000')
                FROM [Suppliers] s
                INNER JOIN #SupplierMap m ON s.Id = m.OldId;
            ");

            // ── Step 4: Add PartyId to Purchases and backfill from mapping ───────
            migrationBuilder.AddColumn<Guid>(
                name: "PartyId",
                table: "Purchases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE p
                SET p.PartyId = m.NewId
                FROM [Purchases] p
                INNER JOIN #SupplierMap m ON p.SupplierId = m.OldId;

                DROP TABLE #SupplierMap;
            ");

            // ── Step 5: Add PartyId to AspNetUsers (initially null) ──────────────
            migrationBuilder.AddColumn<Guid>(
                name: "PartyId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            // ── Step 6: Rename Sales.CustomerId → PartyId (data preserved) ───────
            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Sales",
                newName: "PartyId");

            migrationBuilder.RenameIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales",
                newName: "IX_Sales_PartyId");

            // ── Step 7: Drop old FKs, indexes, and columns ───────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Suppliers_SupplierId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_SupplierId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Purchases");

            // ── Step 8: Drop old tables ───────────────────────────────────────────
            migrationBuilder.DropTable(name: "Customers");
            migrationBuilder.DropTable(name: "Suppliers");

            // ── Step 9: Create indexes ────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PartyId",
                table: "Purchases",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PartyId",
                table: "AspNetUsers",
                column: "PartyId",
                unique: true,
                filter: "[PartyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_BranchId",
                table: "Parties",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Email",
                table: "Parties",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_PartyType",
                table: "Parties",
                column: "PartyType");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_TenantId",
                table: "Parties",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionKey",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionKey" },
                unique: true);

            // ── Step 10: Add new FK constraints ──────────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Parties_PartyId",
                table: "AspNetUsers",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Parties_PartyId",
                table: "Purchases",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Parties_PartyId",
                table: "Sales",
                column: "PartyId",
                principalTable: "Parties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Parties_PartyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Parties_PartyId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Parties_PartyId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_PartyId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PartyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PartyId",
                table: "Sales",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Sales_PartyId",
                table: "Sales",
                newName: "IX_Sales_CustomerId");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SupplierId",
                table: "Purchases",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive",
                table: "Customers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Title",
                table: "Customers",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsActive",
                table: "Suppliers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Title",
                table: "Suppliers",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Suppliers_SupplierId",
                table: "Purchases",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Customers_CustomerId",
                table: "Sales",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
