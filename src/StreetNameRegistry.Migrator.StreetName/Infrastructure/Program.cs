namespace StreetNameRegistry.Migrator.StreetName.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Consumer;
    using Consumer.Municipality;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Polly;
    using Serilog;
    using StreetNameRegistry.StreetName;
    using StreetNameRegistry.StreetName.Commands;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting StreetNameRegistry.Consumer");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await Policy
                                    .Handle<SqlException>()
                                    .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(60),
                                        (_, timespan) => Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                                    .ExecuteAsync(async () =>
                                    {
                                        await ProcessStreams(container, configuration, ct);
                                    });
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetService<ILogger<Program>>()!);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, default);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static async Task ProcessStreams(
            IServiceProvider container,
            IConfigurationRoot configuration,
            CancellationToken cancellationToken)
        {
            var loggerFactory = container.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("StreetNameMigrator");

            var connectionString = configuration.GetConnectionString("events");
            var processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            await processedIdsTable.CreateTableIfNotExists();
            var processedIds = (await processedIdsTable.GetProcessedIds())?.ToList() ?? new List<string>();

            var makeComplete = Convert.ToBoolean(configuration["MakeComplete"]);

            var actualContainer = container.GetRequiredService<ILifetimeScope>();

            var streetNameRepo = actualContainer.Resolve<IStreetNames>();
            var consumerContext = actualContainer.Resolve<ConsumerContext>();
            var backOfficeContext = actualContainer.Resolve<BackOfficeContext>();
            var sqlStreamTable = new SqlStreamsTable(connectionString);

            var streams = (await sqlStreamTable.ReadNextStreetNameStreamPage())?.ToList() ?? new List<string>();

            async Task<bool> ProcessStream(IEnumerable<string> streamsToProcess)
            {
                foreach (var id in streamsToProcess)
                {
                    if (!await ProcessStreamId(processedIds, id, logger, streetNameRepo, makeComplete, consumerContext, actualContainer, processedIdsTable, backOfficeContext, cancellationToken))
                    {
                        return false;
                    }
                }

                return true;
            }

            while (streams.Any())
            {
                if (!await ProcessStream(streams))
                {
                    break;
                }

                streams = (await sqlStreamTable.ReadNextStreetNameStreamPage())?.ToList() ?? new List<string>();
            }
        }

        private static async Task<bool> ProcessStreamId(List<string> processedIds, string id, ILogger logger, IStreetNames streetNameRepo, bool makeComplete, ConsumerContext consumerContext, ILifetimeScope actualContainer, ProcessedIdsTable processedIdsTable, BackOfficeContext backOfficeContext, CancellationToken cancellationToken)
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }

            if (processedIds.Contains(id, StringComparer.InvariantCultureIgnoreCase))
            {
                logger.LogDebug($"Already migrated '{id}', skipping...");
                return true;
            }

            var streetNameId = new StreetNameId(Guid.Parse(id));
            var streetName = await streetNameRepo.GetAsync(streetNameId, cancellationToken);

            if (!streetName.IsCompleted)
            {
                if (streetName.IsRemoved)
                {
                    logger.LogDebug($"Skipping incomplete & removed StreetnameId '{id}'.");
                    return true;
                }

                if (!makeComplete)
                {
                    throw new InvalidOperationException($"Incomplete but not removed Streetname '{id}'.");
                }
            }

            var municipality = await consumerContext.MunicipalityConsumerItems.SingleOrDefaultAsync(x =>
                    x.NisCode == streetName.NisCode, cancellationToken);

            if (municipality == null)
            {
                throw new InvalidOperationException("Municipality for NisCode '{streetName.NisCode}' was not found.");
            }

            await CreateAndDispatchCommand(municipality, streetName, makeComplete, actualContainer, cancellationToken);

            await processedIdsTable.Add(id);
            processedIds.Add(id);

            await backOfficeContext.MunicipalityIdByPersistentLocalId
                .AddAsync(new MunicipalityIdByPersistentLocalId(streetName.PersistentLocalId, municipality.MunicipalityId), cancellationToken);
            await backOfficeContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static async Task CreateAndDispatchCommand(
            MunicipalityConsumerItem municipality,
            StreetName streetName,
            bool makeComplete,
            ILifetimeScope actualContainer,
            CancellationToken ct)
        {
            var municipalityId = new MunicipalityId(municipality.MunicipalityId);
            var migrateCommand = streetName.CreateMigrateCommand(municipalityId, makeComplete);
            var markMigrated = new MarkStreetNameMigrated(municipalityId, new StreetNameId(migrateCommand.StreetNameId), migrateCommand.Provenance);

            await using (var scope = actualContainer.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    markMigrated.CreateCommandId(),
                    markMigrated,
                    cancellationToken: ct);
            }

            await using (var scope = actualContainer.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    migrateCommand.CreateCommandId(),
                    migrateCommand,
                    cancellationToken: ct);
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));

            builder.RegisterModule(new ProjectorModule(configuration));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
