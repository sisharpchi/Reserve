using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using ReserveFlow.Common.Application.Data;

namespace ReserveFlow.Common.Infrastructure.Data;

internal sealed class DatabaseSchemaInitializerHostedService(
    IDatabaseConnectionStringProvider connectionStringProvider,
    IOptions<DatabaseInitializerOptions> options,
    ILogger<DatabaseSchemaInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            LogInitializerDisabled(logger, null);
            return;
        }

        string[] schemas = options.Value.ModuleSchemas
            .Where(schema => !string.IsNullOrWhiteSpace(schema))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (schemas.Length == 0)
        {
            LogNoSchemasConfigured(logger, null);
            return;
        }

        string connectionString = connectionStringProvider.GetConnectionString();

        if (options.Value.CreateDatabaseIfMissing)
        {
            await PostgresDatabaseBootstrapper.EnsureDatabaseExistsAsync(
                connectionString,
                options.Value.MaintenanceDatabase,
                cancellationToken);
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (string schema in schemas)
        {
            await CreateModuleSchemaAsync(connection, schema, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task CreateModuleSchemaAsync(
        NpgsqlConnection connection,
        string schema,
        CancellationToken cancellationToken)
    {
        string quotedSchema = QuoteIdentifier(schema);

        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            create schema if not exists {quotedSchema};
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (identifier.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_'))
        {
            throw new InvalidOperationException($"Invalid PostgreSQL identifier '{identifier}'.");
        }

        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private static readonly Action<ILogger, Exception?> LogInitializerDisabled =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(LogInitializerDisabled)),
            "Database schema initializer is disabled.");

    private static readonly Action<ILogger, Exception?> LogNoSchemasConfigured =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, nameof(LogNoSchemasConfigured)),
            "Database schema initializer has no module schemas configured.");
}
