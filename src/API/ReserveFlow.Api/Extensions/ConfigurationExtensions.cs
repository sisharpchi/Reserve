namespace ReserveFlow.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static IConfigurationBuilder AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IReadOnlyCollection<string> modules)
    {
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", optional: true, reloadOnChange: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }
}
