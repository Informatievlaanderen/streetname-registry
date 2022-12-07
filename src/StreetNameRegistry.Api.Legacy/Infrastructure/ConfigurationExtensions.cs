namespace StreetNameRegistry.Api.Legacy.Infrastructure
{
    using FeatureToggles;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static bool GetFeatureToggle(this IConfiguration configuration, string configurationKey)
        {
            var toggle = new UseProjectionsV2Toggle(false);
            configuration.GetSection(configurationKey).Bind(toggle);

            return toggle.FeatureEnabled;
        }
    }
}
