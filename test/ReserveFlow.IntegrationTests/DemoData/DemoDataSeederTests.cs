using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ReserveFlow.IntegrationTests.DemoData;

public sealed class DemoDataSeederTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("reserveflow_demo_seed_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    [Fact]
    public async Task DemoSeedCreatesTenantCatalogStaffResourceScheduleAndBookingPolicy()
    {
        await using var factory = new DemoSeedApiFactory(_postgres.GetConnectionString());

        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        Guid tenantId = await QuerySingleAsync<Guid>(
            connection,
            "select id from platform.tenants where slug = 'smile-dental' and status = 'Active';");

        Assert.NotEqual(Guid.Empty, tenantId);

        Assert.Equal(2, await QuerySingleAsync<long>(
            connection,
            """
            select count(*)
            from catalog.services
            where tenant_id = @tenant_id
              and name in ('Dental Consultation', 'Teeth Cleaning');
            """,
            tenantId));

        Assert.Equal(1, await QuerySingleAsync<long>(
            connection,
            """
            select count(*)
            from staffing.staff_members
            where tenant_id = @tenant_id
              and email = 'dr.ali@smile-dental.example';
            """,
            tenantId));

        Assert.Equal(1, await QuerySingleAsync<long>(
            connection,
            """
            select count(*)
            from resources.resources
            where tenant_id = @tenant_id
              and name = 'Room 1'
              and resource_type = 'room';
            """,
            tenantId));

        Assert.Equal(10, await QuerySingleAsync<long>(
            connection,
            """
            select count(*)
            from scheduling.working_hours
            where tenant_id = @tenant_id
              and starts_at = '09:00'
              and ends_at = '18:00';
            """,
            tenantId));

        Assert.Equal(1, await QuerySingleAsync<long>(
            connection,
            """
            select count(*)
            from bookings.booking_policies
            where tenant_id = @tenant_id
              and minimum_advance_minutes = 60
              and cancellation_deadline_hours = 12;
            """,
            tenantId));
    }

    public Task InitializeAsync()
    {
        return _postgres.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgres.DisposeAsync().AsTask();
    }

    private static async Task<T> QuerySingleAsync<T>(
        NpgsqlConnection connection,
        string sql,
        Guid? tenantId = null)
    {
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = sql;

        if (tenantId.HasValue)
        {
            command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        }

        object? value = await command.ExecuteScalarAsync();

        Assert.NotNull(value);

        return (T)value;
    }

    private sealed class DemoSeedApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                Dictionary<string, string?> configuration = new()
                {
                    ["ConnectionStrings:Database"] = connectionString,
                    ["DatabaseInitializer:Enabled"] = "true",
                    ["DatabaseInitializer:CreateDatabaseIfMissing"] = "false",
                    ["Tenants:SeedDemoTenant"] = "true",
                    ["Bookings:Outbox:Enabled"] = "false",
                    ["Notifications:Delivery:Enabled"] = "false",
                    ["Keycloak:HealthUrl"] = string.Empty,
                    ["OpenTelemetry:Otlp:Enabled"] = "false"
                };

                configurationBuilder.AddInMemoryCollection(configuration);
            });
        }
    }
}
