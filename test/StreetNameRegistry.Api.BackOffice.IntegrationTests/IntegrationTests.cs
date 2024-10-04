namespace StreetNameRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen", Scopes.DvArAdresBeheer)]
        [InlineData("/v2/straatnamen/acties/oslosnapshots", $"{Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren", Scopes.DvArAdresBeheer)]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/homoniemtoevoeging", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren", Scopes.DvArAdresBeheer)]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/opheffen", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/hernoemen", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam", Scopes.DvArAdresBeheer)]
        [InlineData("/v2/straatnamen/1/acties/wijzigen/straatnaam", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        [InlineData("/v2/straatnamen/1/acties/verwijderen", $"{Scopes.DvArAdresBeheer} {Scopes.DvArAdresUitzonderingen}")]
        public async Task ReturnsSuccess(string endpoint, string requiredScopes)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen")]
        [InlineData("/v2/straatnamen/acties/oslosnapshots")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/homoniemtoevoeging")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/straatnamen/1/acties/opheffen")]
        [InlineData("/v2/straatnamen/1/acties/hernoemen")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam")]
        [InlineData("/v2/straatnamen/1/acties/wijzigen/straatnaam")]
        [InlineData("/v2/straatnamen/1/acties/verwijderen")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/straatnamen/acties/voorstellen")]
        [InlineData("/v2/straatnamen/acties/oslosnapshots")]
        [InlineData("/v2/straatnamen/1/acties/goedkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/goedkeuring")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/homoniemtoevoeging")]
        [InlineData("/v2/straatnamen/1/acties/afkeuren")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/afkeuring")]
        [InlineData("/v2/straatnamen/1/acties/opheffen")]
        [InlineData("/v2/straatnamen/1/acties/hernoemen")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/opheffing")]
        [InlineData("/v2/straatnamen/1/acties/corrigeren/straatnaam")]
        [InlineData("/v2/straatnamen/1/acties/wijzigen/straatnaam")]
        [InlineData("/v2/straatnamen/1/acties/verwijderen")]
        public async Task ReturnsForbidden(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken());

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
