namespace StreetNameRegistry.Importer.Console
{
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CommandLine;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using Crab;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    internal class Program
    {
        private static Stopwatch _stopwatch;
        private static int _commandCounter;

        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var mailSettings = configuration.GetSection("ApplicationSettings").GetSection("SerilogMail");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("tracing.log", LogEventLevel.Verbose)
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.SendGridSmtp(
                    mailSettings["apiKey"],
                    mailSettings["subject"],
                    mailSettings["fromEmail"],
                    mailSettings["toEmail"],
                    (LogEventLevel) Enum.Parse(typeof(LogEventLevel), mailSettings["restrictedToMinimumLevel"], true))
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var crabConnectionString = configuration.GetConnectionString("CRABEntities");
            Func<CRABEntities> crabEntitiesFactory = () => new CRABEntities(crabConnectionString);

            var settings = new SettingsBasedConfig(configuration.GetSection("ApplicationSettings"));
            try
            {
                var options = new ImportOptions(
                    args,
                    errors => WaitForExit(settings, "Could not parse commandline options."));

                MapLogging.Log = s => _commandCounter++;

                var commandProcessor = new CommandProcessorBuilder<int>(new StreetNameCommandGenerator(crabEntitiesFactory))
                    .WithCommandLineOptions(options.ImportArguments)
                    .UseSerilog(cfg => cfg
                        .WriteTo.File("tracing.log", LogEventLevel.Verbose)
                        .WriteTo.Console(LogEventLevel.Information))
                    .UseHttpApiProxyConfig(settings)
                    .UseCommandProcessorConfig(settings)
                    .UseDefaultSerializerSettingsForCrabImports()
                    .ConfigureImportFeedFromAssembly(Assembly.GetExecutingAssembly())
                    //.UseApiProxyFactory(FileBasedProxyFactory.BuildFileBasedProxyFactory)
                    .Build();

                WaitForStart(settings);

                commandProcessor.Run(options, settings);

                WaitForExit(settings);
            }
            catch (Exception exception)
            {
                WaitForExit(settings, "General error occurred", exception);
            }
        }

        private static void WaitForExit(ICommandProcessorConfig settings, string errorMessage = null, Exception exception = null)
        {
            if (!string.IsNullOrEmpty(errorMessage) || exception != null)
                Log.Fatal(exception, errorMessage);

            Console.WriteLine();

            if (_stopwatch != null)
            {
                var avg = _commandCounter / _stopwatch.Elapsed.TotalSeconds;
                var summary = $"Report: generated {_commandCounter} commands in {_stopwatch.Elapsed}ms (={avg}/second).";
                Console.WriteLine(summary);
            }

            if (settings.WaitForUserInput)
            {
                Console.WriteLine("Done! Press ENTER key to exit...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }

            if (!string.IsNullOrEmpty(errorMessage))
                Environment.Exit(1);

            Environment.Exit(0);
        }

        private static void WaitForStart(ICommandProcessorConfig settings)
        {
            if (settings.WaitForUserInput)
            {
                Console.WriteLine("Press ENTER key to start the CRAB Import...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }

            _stopwatch = Stopwatch.StartNew();
        }
    }
}
