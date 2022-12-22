namespace StreetNameRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<MunicipalityIdByPersistentLocalId> MunicipalityIdByPersistentLocalId { get; set; }

        public async Task<MunicipalityIdByPersistentLocalId> AddIdempotentMunicipalityStreetNameIdRelation(int streetNamePersistentLocalId, Guid municipalityId, CancellationToken cancellationToken)
        {
            var relation = await MunicipalityIdByPersistentLocalId.FindAsync(new object?[] { streetNamePersistentLocalId }, cancellationToken: cancellationToken);
            if (relation is null)
            {
                relation = new MunicipalityIdByPersistentLocalId(streetNamePersistentLocalId, municipalityId);
                await MunicipalityIdByPersistentLocalId.AddAsync(relation, cancellationToken);
            }

            return relation;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MunicipalityIdByPersistentLocalId>()
                .ToTable(nameof(MunicipalityIdByPersistentLocalId), Schema.BackOffice)
                .HasKey(x => x.PersistentLocalId)
                .IsClustered();

            modelBuilder.Entity<MunicipalityIdByPersistentLocalId>()
                .Property(x => x.PersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<MunicipalityIdByPersistentLocalId>()
                .Property(x => x.MunicipalityId);
        }
    }

    public class MunicipalityIdByPersistentLocalId
    {
        public int PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }

        private MunicipalityIdByPersistentLocalId()
        { }

        public MunicipalityIdByPersistentLocalId(int persistentLocalId, Guid municipalityId)
        {
            PersistentLocalId = persistentLocalId;
            MunicipalityId = municipalityId;
        }
    }

    public class ConfigBasedSequenceContextFactory : IDesignTimeDbContextFactory<BackOfficeContext>
    {
        public BackOfficeContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "BackOffice";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<BackOfficeContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice);
                });

            return new BackOfficeContext(builder.Options);
        }
    }
}
