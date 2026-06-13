using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Modules.Identity.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialIdentity : Migration
{
    private static readonly string[] RolePermissionColumns = ["role_id", "permission_id"];
    private static readonly string[] TenantUserColumns = ["tenant_id", "user_id"];
    private static readonly string[] UserRoleTenantColumns = ["tenant_id", "role_id"];
    private static readonly string[] UserRoleColumns = ["user_id", "role_id", "tenant_id"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "identity");

        migrationBuilder.CreateTable(
            name: "permissions",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_permissions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "role_permissions",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role_permissions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "roles",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                is_platform_role = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenant_users",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tenant_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_roles",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                keycloak_subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_permissions_code",
            schema: "identity",
            table: "permissions",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_role_permissions_permission_id",
            schema: "identity",
            table: "role_permissions",
            column: "permission_id");

        migrationBuilder.CreateIndex(
            name: "IX_role_permissions_role_id_permission_id",
            schema: "identity",
            table: "role_permissions",
            columns: RolePermissionColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_roles_name",
            schema: "identity",
            table: "roles",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_tenant_users_tenant_id_user_id",
            schema: "identity",
            table: "tenant_users",
            columns: TenantUserColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_tenant_users_user_id",
            schema: "identity",
            table: "tenant_users",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_user_roles_tenant_id_role_id",
            schema: "identity",
            table: "user_roles",
            columns: UserRoleTenantColumns);

        migrationBuilder.CreateIndex(
            name: "IX_user_roles_user_id_role_id_tenant_id",
            schema: "identity",
            table: "user_roles",
            columns: UserRoleColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_users_email",
            schema: "identity",
            table: "users",
            column: "email");

        migrationBuilder.CreateIndex(
            name: "IX_users_keycloak_subject",
            schema: "identity",
            table: "users",
            column: "keycloak_subject",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "permissions",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "role_permissions",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "roles",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "tenant_users",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "user_roles",
            schema: "identity");

        migrationBuilder.DropTable(
            name: "users",
            schema: "identity");
    }
}
