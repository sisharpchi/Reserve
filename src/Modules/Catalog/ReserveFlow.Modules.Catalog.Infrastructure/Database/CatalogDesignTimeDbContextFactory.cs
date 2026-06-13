using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReserveFlow.Modules.Catalog.Infrastructure.Database;

public sealed class CatalogDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=reserveflow;Username=postgres;Password=postgres";

    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        optionsBuilder.UseNpgsql(
            GetConnectionString(args),
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                Schemas.Catalog));

        return new CatalogDbContext(optionsBuilder.Options);
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
