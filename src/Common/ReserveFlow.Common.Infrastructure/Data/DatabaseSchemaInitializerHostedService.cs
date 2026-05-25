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

        await using var connection = new NpgsqlConnection(connectionStringProvider.GetConnectionString());
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

            create table if not exists {quotedSchema}.outbox_messages (
                id uuid primary key,
                tenant_id uuid null,
                type varchar(500) not null,
                payload jsonb not null,
                occurred_on_utc timestamptz not null,
                processed_on_utc timestamptz null,
                error text null,
                retry_count int not null default 0
            );

            alter table {quotedSchema}.outbox_messages
                add column if not exists tenant_id uuid null;

            alter table {quotedSchema}.outbox_messages
                add column if not exists payload jsonb null;

            alter table {quotedSchema}.outbox_messages
                add column if not exists retry_count int not null default 0;

            create table if not exists {quotedSchema}.outbox_message_consumers (
                id uuid primary key,
                name varchar(500) not null
            );

            create table if not exists {quotedSchema}.inbox_messages (
                id uuid primary key,
                type varchar(500) not null,
                payload jsonb not null,
                occurred_on_utc timestamptz not null,
                processed_on_utc timestamptz null,
                error text null
            );

            alter table {quotedSchema}.inbox_messages
                add column if not exists payload jsonb null;

            create table if not exists {quotedSchema}.inbox_message_consumers (
                id uuid primary key,
                name varchar(500) not null
            );
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
