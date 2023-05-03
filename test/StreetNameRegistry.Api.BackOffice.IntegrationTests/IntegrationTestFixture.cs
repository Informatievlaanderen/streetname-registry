namespace StreetNameRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using Ductus.FluentDocker.Services;
    using IdentityModel;
    using IdentityModel.Client;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class IntegrationTestFixture : IAsyncLifetime
    {
        private string _clientId;
        private string _clientSecret;
        private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();
        private IConfigurationRoot? _configuration;
        private ICompositeService _sqlService;

        public ITestOutputHelper? TestOutputHelper { get; set; }

        public Lazy<TestServer> TestServer => new Lazy<TestServer>(() =>
        {
            var hostBuilder = new WebHostBuilder()
                .UseConfiguration(_configuration)
                .UseStartup<Startup>()
                .UseTestServer()
                .UseEnvironment("Development")
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new XunitTestOutputLoggerProvider(TestOutputHelper));
                });

            return new TestServer(hostBuilder);
        });
        public SqlConnection SqlConnection { get; private set; }

        public async Task<string> GetAccessToken(string requiredScopes = "")
        {
            if (_accessTokens.ContainsKey(requiredScopes) && !_accessTokens[requiredScopes].IsExpired)
            {
                return _accessTokens[requiredScopes].Token;
            }

            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessTokens[requiredScopes] = new AccessToken(response.AccessToken, response.ExpiresIn);

            return _accessTokens[requiredScopes].Token;
        }

        public async Task InitializeAsync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = _configuration.GetValue<string>("ClientId");
            _clientSecret = _configuration.GetValue<string>("ClientSecret");

            _sqlService = DockerComposer.Compose("sqlserver.yml", "streetname-integration-tests");
            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();

            // This is necessary for Migration StreetNameRegistry.Api.BackOffice.Abstractions.Migrations.AddNisCode
            // We don't want to run this migration in the BackOffice.Api itself.
            await Consumer.Infrastructure.MigrationsHelper.RunAsync(
                _configuration.GetConnectionString("ConsumerAdmin"),
                NullLoggerFactory.Instance,
                CancellationToken.None);
        }

        private async Task WaitForSqlServerToBecomeAvailable()
        {
            foreach (var _ in Enumerable.Range(0, 60))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (await OpenConnection())
                {
                    break;
                }
            }
        }

        private async Task<bool> OpenConnection()
        {
            try
            {
                SqlConnection = new SqlConnection("Server=localhost,5434;User Id=sa;Password=Pass@word;database=master;TrustServerCertificate=True;");
                await SqlConnection.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task CreateDatabase()
        {
            var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'streetname-registry') BEGIN CREATE DATABASE [streetname-registry] END", SqlConnection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DisposeAsync()
        {
            await SqlConnection.DisposeAsync();
            _sqlService.Dispose();
            TestOutputHelper = null;
        }
    }

    public class AccessToken
    {
        private readonly DateTime _expiresAt;

        public string Token { get; }

        // Let's regard it as expired 10 seconds before it actually expires.
        public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

        public AccessToken(string token, int expiresIn)
        {
            _expiresAt = DateTime.Now.AddSeconds(expiresIn);
            Token = token;
        }
    }

    public class XunitTestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XunitTestOutputLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitTestOutputLogger(_output);
        }

        public void Dispose() { }
    }

    public class XunitTestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper _output;

        public XunitTestOutputLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            _output.WriteLine(message);
        }
    }
}
