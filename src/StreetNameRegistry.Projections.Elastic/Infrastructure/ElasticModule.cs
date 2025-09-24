namespace StreetNameRegistry.Projections.Elastic.Infrastructure
{
    using System;
    using Autofac;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Transport;
    using Microsoft.Extensions.Configuration;
    using StreetNameList;

    public class ElasticModule : Module
    {
        public const string ConfigurationSectionName = "Elastic";

        private readonly IConfiguration _configuration;

        public ElasticModule(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var elasticOptions = _configuration.GetSection(ConfigurationSectionName);

            var clientSettings = new ElasticsearchClientSettings(new Uri(elasticOptions["Uri"]!));
            if (elasticOptions.GetValue<bool>("DebugMode"))
            {
                clientSettings.EnableDebugMode();
                clientSettings.DisableDirectStreaming();
            }

            var apiKey = elasticOptions.GetValue("ApiKey", string.Empty);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                clientSettings = clientSettings.Authentication(new ApiKey(apiKey));
            }
            else
            {
                var username = elasticOptions.GetValue("Username", string.Empty);
                var password = elasticOptions.GetValue("Password", string.Empty);
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    clientSettings = clientSettings.Authentication(new BasicAuthentication(username, password));
                }
            }

            builder
                .Register<ElasticsearchClient>(_ => new ElasticsearchClient(clientSettings))
                .SingleInstance();

            builder.Register<IStreetNameListElasticClient>(c =>
                    new StreetNameListElasticClient(
                        c.Resolve<ElasticsearchClient>(),
                        c.Resolve<IConfiguration>().GetSection(ConfigurationSectionName)["ListIndexName"]!))
                .SingleInstance();
        }
    }
}
