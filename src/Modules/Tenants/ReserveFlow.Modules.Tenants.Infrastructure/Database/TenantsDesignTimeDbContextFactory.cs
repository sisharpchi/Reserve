using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReserveFlow.Modules.Tenants.Infrastructure.Database;

public sealed class TenantsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantsDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=reserveflow;Username=postgres;Password=postgres";

    public TenantsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();

        optionsBuilder.UseNpgsql(
            GetConnectionString(args),
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                Schemas.Platform));

        return new TenantsDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString(string[] args)
    {
        string? argumentConnectionString = args
            .FirstOrDefault(argument => argument.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=', count: 2)[1];

        return FirstNotEmpty(
            argumentConnectionString,
            Environment.GetEnvironmentVariable("ConnectionStrings__Database"),
            Environment.GetEnvironmentVariable("RESERVEFLOW_DATABASE_CONNECTION_STRING"),
            DefaultConnectionString);
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return DefaultConnectionString;
    }
}
