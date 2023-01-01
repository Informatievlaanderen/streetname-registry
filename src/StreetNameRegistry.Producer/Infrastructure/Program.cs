namespace StreetNameRegistry.Producer.Infrastructure
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using global::Microsoft.AspNetCore.Hosting;

    public class Program
    {
        protected Program()
        { }

        public static void Main(string[] args)
            => Run(new ProgramOptions
                {
                    Hosting =
                    {
                        HttpPort = 4014
                    },
                    Logging =
                    {
                        WriteTextToConsole = false,
                        WriteJsonToConsole = false
                    },
                    Runtime =
                    {
                        CommandLineArgs = args
                    },
                    MiddlewareHooks =
                    {
                        ConfigureDistributedLock = DistributedLockOptions.LoadFromConfiguration
                    }
                });

        private static void Run(ProgramOptions options)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(options)
                .RunWithLock<Program>();
    }
}
