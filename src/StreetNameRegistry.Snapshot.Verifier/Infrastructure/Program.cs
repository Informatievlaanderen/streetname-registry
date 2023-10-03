namespace StreetNameRegistry.Snapshot.Verifier.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.SnapshotVerifier;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Municipality;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;
    using StreetNameRegistry.Infrastructure;

    public sealed class Program
    {
        private Program()
        {
        }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Initializing StreetNameRegistry.Snapshot.Verifier");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

                    Log.Logger = new LoggerConfiguration() //NOSONAR logging configuration is safe
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();

                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSnapshotVerificationServices(hostContext.Configuration.GetConnectionString("Snapshots"), Schema.Default);
                    var config = DefaultComparisonConfig.Instance;
                    config.MembersToIgnore.AddRange(new List<string> { "_lastSnapshotEventHash", "_lastSnapshotProvenance", "OfficialLanguages", "FacilityLanguages" });
                    config.CollectionMatchingSpec.Add(typeof(MunicipalityStreetName), new []{nameof(MunicipalityStreetName.PersistentLocalId)});
                    config.CollectionMatchingSpec.Add(typeof(StreetNameHomonymAddition), new []{nameof(StreetNameHomonymAddition.HomonymAddition), nameof(StreetNameHomonymAddition.Language)});
                    config.CollectionMatchingSpec.Add(typeof(StreetNameName), new []{nameof(StreetNameName.Name), nameof(StreetNameName.Language)});
                    services.AddHostedSnapshotVerifierService<Municipality, MunicipalityStreamId>(
                        () => new MunicipalityFactory(NoSnapshotStrategy.Instance).Create(),
                        aggregate => new MunicipalityStreamId(aggregate.MunicipalityId),
                        config);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger); //NOSONAR logging configuration is safe

                    containerBuilder.RegisterModule(new ApiModule(hostContext.Configuration, services, loggerFactory));
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting StreetNameRegistry.Snapshot.Verifier");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                        async () =>
                        {
                            host.Services.GetRequiredService<ISnapshotVerificationRepository>().EnsureCreated();
                            await host.RunAsync().ConfigureAwait(false);
                        },
                        DistributedLockOptions.LoadFromConfiguration(configuration),
                        logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(500, default);
                throw;
            }
            finally
            {
                logger.LogInformation("Stopping...");
            }
        }
    }
}
