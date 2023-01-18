namespace StreetNameRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.AspNetCore.OAuth2Introspection;
    using IdentityModel.Client;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private OAuth2IntrospectionOptions _oAuth2IntrospectionOptions;
        private readonly TestServer _testServer;

        public IntegrationTests()
        {
            var hostBuilder = new WebHostBuilder().ConfigureAppConfiguration(configurationBuilder =>
            {
                var configuration = configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                _oAuth2IntrospectionOptions = configuration
                    .GetSection(nameof(OAuth2IntrospectionOptions))
                    .Get<OAuth2IntrospectionOptions>()!;
            })
            .UseStartup<Startup>()
            .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
            .UseTestServer();

            _testServer = new TestServer(hostBuilder);
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen", "dv_ar_adres_beheer")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren", "dv_ar_adres_beheer")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring", "dv_ar_adres_beheer dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren", "dv_ar_adres_beheer")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring", "dv_ar_adres_beheer dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/straatnamen/1/acties/opheffen", "dv_ar_adres_beheer")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing", "dv_ar_adres_beheer dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam", "dv_ar_adres_beheer")]
        public async Task ReturnsSuccess(string endpoint, string requiredScopes)
        {
            var client = _testServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken(requiredScopes));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/straatnamen/1/acties/opheffen")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _testServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/straatnamen/1/acties/opheffen")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam")]
        public async Task ReturnsForbidden(string endpoint)
        {
            var client = _testServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private async Task<string> GetAccessToken(string? requiredScopes = null)
        {
            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _oAuth2IntrospectionOptions.ClientId,
                    ClientSecret = _oAuth2IntrospectionOptions.ClientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes ?? string.Empty) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            return response.AccessToken;
        }
    }
}
