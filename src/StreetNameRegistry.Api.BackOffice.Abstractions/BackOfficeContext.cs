namespace StreetNameRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<MunicipalityIdByPersistentLocalId> MunicipalityIdByPersistentLocalId => Set<MunicipalityIdByPersistentLocalId>();

        public async Task<MunicipalityIdByPersistentLocalId> AddIdempotentMunicipalityStreetNameIdRelation(
            int streetNamePersistentLocalId,
            Guid municipalityId,
            string nisCode,
            CancellationToken cancellationToken)
        {
            var relation = await MunicipalityIdByPersistentLocalId.FindAsync(new object?[] { streetNamePersistentLocalId }, cancellationToken: cancellationToken);
            if (relation is not null)
            {
                return relation;
            }

            try
            {
                relation = new MunicipalityIdByPersistentLocalId(streetNamePersistentLocalId, municipalityId, nisCode);
                await MunicipalityIdByPersistentLocalId.AddAsync(relation, cancellationToken);

                await SaveChangesAsync(cancellationToken);
            }
            catch(DbUpdateException exception)
            {
                // It can happen that the back office projections were faster adding the relation than the executor (or vice versa).
                if (exception.InnerException is not SqlException { Number: 2627 })
                {
                    throw;
                }

                relation = await MunicipalityIdByPersistentLocalId.FirstOrDefaultAsync(
                    x => x.MunicipalityId == municipalityId, cancellationToken);

                if (relation is null)
                {
                    throw;
                }
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

            modelBuilder.Entity<MunicipalityIdByPersistentLocalId>()
                .Property(x => x.NisCode);

            modelBuilder.Entity<MunicipalityIdByPersistentLocalId>()
                .HasIndex(x => x.NisCode);
        }
    }

    public class MunicipalityIdByPersistentLocalId
    {
        public int PersistentLocalId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string NisCode { get; set; }

        private MunicipalityIdByPersistentLocalId()
        { }

        public MunicipalityIdByPersistentLocalId(int persistentLocalId, Guid municipalityId, string nisCode)
        {
            PersistentLocalId = persistentLocalId;
            MunicipalityId = municipalityId;
            NisCode = nisCode;
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
