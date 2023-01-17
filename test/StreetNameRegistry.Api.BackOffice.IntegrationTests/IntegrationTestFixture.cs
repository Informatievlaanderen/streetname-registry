namespace StreetNameRegistry.Api.BackOffice.IntegrationTests
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Services;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public class IntegrationTestFixture : IAsyncLifetime
    {
        private static readonly Lazy<ICompositeService> BackOfficeApiContainers =
            new Lazy<ICompositeService>(Tests.ContainerHelper.BackOfficeApiContainers.Compose);

        public SqlConnection SqlConnection { get; private set; }

        public async Task InitializeAsync()
        {
            // Invoke the lazy member for it be initialized and run the docker container.
            _ = BackOfficeApiContainers.Value;

            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();
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
            if (SqlConnection.State == ConnectionState.Open)
            {
                await SqlConnection.CloseAsync();
            }

            await SqlConnection.DisposeAsync();
        }
    }
}
