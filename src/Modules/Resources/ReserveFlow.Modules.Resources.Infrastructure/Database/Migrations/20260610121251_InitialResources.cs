using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Resources.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialResources : Migration
{
    private static readonly string[] TenantActiveColumns = ["tenant_id", "is_active"];
    private static readonly string[] TenantTypeColumns = ["tenant_id", "resource_type"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "resources");

        migrationBuilder.CreateTable(
            name: "resources",
            schema: "resources",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                resource_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                capacity = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_resources", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_resources_tenant_active",
            schema: "resources",
            table: "resources",
            columns: TenantActiveColumns);

        migrationBuilder.CreateIndex(
            name: "ix_resources_tenant_type",
            schema: "resources",
            table: "resources",
            columns: TenantTypeColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "resources",
            schema: "resources");
    }
}
