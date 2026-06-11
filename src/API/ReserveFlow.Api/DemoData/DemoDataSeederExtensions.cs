using Microsoft.Extensions.DependencyInjection;

namespace ReserveFlow.Api.DemoData;

internal static class DemoDataSeederExtensions
{
    internal static IServiceCollection AddDemoDataSeeder(this IServiceCollection services)
    {
        services.AddHostedService<DemoDataSeederHostedService>();

        return services;
    }
}
