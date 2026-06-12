using System.Net.Sockets;
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

        DatabaseInitializerOptions initializerOptions = options.Value;

        string[] schemas = initializerOptions.ModuleSchemas
            .Where(schema => !string.IsNullOrWhiteSpace(schema))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (schemas.Length == 0)
        {
            LogNoSchemasConfigured(logger, null);
            return;
        }

        string connectionString = connectionStringProvider.GetConnectionString();

        if (initializerOptions.CreateDatabaseIfMissing)
        {
            await ExecuteWithStartupRetryAsync(
                () => PostgresDatabaseBootstrapper.EnsureDatabaseExistsAsync(
                    connectionString,
                    initializerOptions.MaintenanceDatabase,
                    cancellationToken),
                "database bootstrap",
                initializerOptions,
                cancellationToken);
        }

        await ExecuteWithStartupRetryAsync(
            () => CreateModuleSchemasAsync(connectionString, schemas, cancellationToken),
            "schema initialization",
            initializerOptions,
            cancellationToken);
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

    private async Task ExecuteWithStartupRetryAsync(
        Func<Task> operation,
        string operationName,
        DatabaseInitializerOptions initializerOptions,
        CancellationToken cancellationToken)
    {
        int attempts = initializerOptions.GetSafeStartupRetryAttempts();
        TimeSpan delay = initializerOptions.GetSafeStartupRetryDelay();

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception exception) when (
                attempt < attempts &&
                IsTransientPostgresStartupException(exception) &&
                !cancellationToken.IsCancellationRequested)
            {
                LogInitializerRetrying(
                    logger,
                    operationName,
                    attempt,
                    attempts,
                    (int)delay.TotalMilliseconds,
                    exception);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static async Task CreateModuleSchemasAsync(
        string connectionString,
        string[] schemas,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (string schema in schemas)
        {
            await CreateModuleSchemaAsync(connection, schema, cancellationToken);
        }
    }

    private static bool IsTransientPostgresStartupException(Exception exception)
    {
        if (exception is PostgresException postgresException)
        {
            return postgresException.SqlState == "57P03";
        }

        if (exception is NpgsqlException { InnerException: SocketException or TimeoutException })
        {
            return true;
        }

        return exception is TimeoutException ||
               (exception.InnerException is not null && IsTransientPostgresStartupException(exception.InnerException));
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

    private static readonly Action<ILogger, string, int, int, int, Exception?> LogInitializerRetrying =
        LoggerMessage.Define<string, int, int, int>(
            LogLevel.Warning,
            new EventId(3, nameof(LogInitializerRetrying)),
            "Database schema initializer {OperationName} failed during PostgreSQL startup. Retrying attempt {Attempt}/{Attempts} in {DelayMilliseconds} ms.");
}
