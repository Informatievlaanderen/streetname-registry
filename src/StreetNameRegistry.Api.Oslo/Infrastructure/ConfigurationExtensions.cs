namespace StreetNameRegistry.Api.Oslo.Infrastructure
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        internal class FeatureToggle
        {
            public bool UseProjectionsV2 { get; set; }
        }

        public static bool GetFeatureToggle(this IConfiguration configuration, string configurationKey)
        {
            var toggle = new FeatureToggle();
            configuration.GetSection(configurationKey).Bind(toggle);

            return toggle.UseProjectionsV2;
        }
    }
}
