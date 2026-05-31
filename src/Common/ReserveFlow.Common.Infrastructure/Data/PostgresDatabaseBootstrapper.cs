using Npgsql;

namespace ReserveFlow.Common.Infrastructure.Data;

internal static class PostgresDatabaseBootstrapper
{
    public static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        string maintenanceDatabase,
        CancellationToken cancellationToken)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
        {
            throw new InvalidOperationException("Connection string must include a target PostgreSQL database.");
        }

        string targetDatabase = targetBuilder.Database;

        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = string.IsNullOrWhiteSpace(maintenanceDatabase)
                ? "postgres"
                : maintenanceDatabase
        };

        await using var connection = new NpgsqlConnection(maintenanceBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        if (await DatabaseExistsAsync(connection, targetDatabase, cancellationToken))
        {
            return;
        }

        await using NpgsqlCommand createCommand = connection.CreateCommand();
        createCommand.CommandText = $"create database {QuoteIdentifier(targetDatabase)}";

        try
        {
            await createCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == "42P04")
        {
            // Another initializer may have created the database after our existence check.
        }
    }

    private static async Task<bool> DatabaseExistsAsync(
        NpgsqlConnection connection,
        string database,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "select 1 from pg_database where datname = @database";
        command.Parameters.AddWithValue("database", database);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is not null;
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (identifier.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_'))
        {
            throw new InvalidOperationException($"Invalid PostgreSQL identifier '{identifier}'.");
        }

        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
