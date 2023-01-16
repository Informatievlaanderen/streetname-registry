namespace StreetNameRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IdentityModel;
    using IdentityModel.AspNetCore.OAuth2Introspection;
    using IdentityModel.Client;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class IntegrationTests
    {
        private readonly IConfigurationRoot _configuration;
        private readonly OAuth2IntrospectionOptions _oAuth2IntrospectionOptions;

        public IntegrationTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true)
                .Build();

            _oAuth2IntrospectionOptions =
                _configuration.GetSection(nameof(OAuth2IntrospectionOptions)).Get<OAuth2IntrospectionOptions>()!;
        }

        [Theory]
        [InlineData("dv_ar_adres_beheer")]
        public async Task GivenProposeAddress_WithCorrectClaims(string scopes)
        {
            var accessToken = await GetAccessToken(scopes);
            var response = await RunWebApiSample().PostAsync("/v2/straatnamen/acties/voorstellen", "{}", accessToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private TestServer RunWebApiSample()
        {
            var hostBuilder = new WebHostBuilder();
            hostBuilder.UseConfiguration(_configuration);
            hostBuilder.UseStartup<Startup>();
            hostBuilder.ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole());
            hostBuilder.UseTestServer();
            return new TestServer(hostBuilder);
        }

        private async Task<string> GetAccessToken(string scopes)
        {
            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = $"https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _oAuth2IntrospectionOptions.ClientId,
                    ClientSecret = _oAuth2IntrospectionOptions.ClientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", scopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            return response.AccessToken;
        }
    }

    public static class TestServerExtensions
    {
        public static async Task<HttpResponseMessage> PostAsync(this TestServer webApi, string requestUri, string jsonBody, string accessToken)
        {
            var httpClient = webApi.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            return await httpClient.PostAsync(requestUri, content);
        }
    }
}
