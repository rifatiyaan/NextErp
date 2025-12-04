using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextErp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9638), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9641), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9643), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9645), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9647), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9648), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9650), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9652), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9653), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9655), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9817), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9822), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9824), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9827), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9829), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9831), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9833), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9835), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9838), new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "BranchId", "CreatedAt", "TenantId" },
                values: new object[] { null, new DateTime(2025, 10, 3, 17, 0, 57, 824, DateTimeKind.Utc).AddTicks(9840), new Guid("00000000-0000-0000-0000-000000000000") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5336));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5338));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5340));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5342));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5344));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5345));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5347));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5349));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5350));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5352));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5517));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5521));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5524));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5526));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5528));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5530));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5532));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5535));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5537));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 9, 0, 28, 369, DateTimeKind.Utc).AddTicks(5539));
        }
    }
}
