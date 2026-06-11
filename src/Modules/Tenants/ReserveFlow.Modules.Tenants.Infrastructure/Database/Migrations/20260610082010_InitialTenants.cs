using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Tenants.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialTenants : Migration
{
    private static readonly string[] CategoryActiveSortColumns = ["is_active", "sort_order"];
    private static readonly string[] TenantCategoryStatusColumns = ["category_id", "status"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "platform");

        migrationBuilder.CreateTable(
            name: "categories",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_categories", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            schema: "platform",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tenants", x => x.id);
                table.ForeignKey(
                    name: "FK_tenants_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "platform",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_categories_active_sort",
            schema: "platform",
            table: "categories",
            columns: CategoryActiveSortColumns);

        migrationBuilder.CreateIndex(
            name: "ux_categories_slug",
            schema: "platform",
            table: "categories",
            column: "slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenants_category_status",
            schema: "platform",
            table: "tenants",
            columns: TenantCategoryStatusColumns);

        migrationBuilder.CreateIndex(
            name: "ix_tenants_status",
            schema: "platform",
            table: "tenants",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ux_tenants_slug",
            schema: "platform",
            table: "tenants",
            column: "slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tenants",
            schema: "platform");

        migrationBuilder.DropTable(
            name: "categories",
            schema: "platform");
    }
}
