namespace StreetNameRegistry
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Autofac;
    using Autofac.Core;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Microsoft.Extensions.DependencyInjection;
    using Municipality;
    using StreetName;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<CrabStreetNameProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<StreetNameLegacyProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<CrabStreetNameCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(CrabStreetNameCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<StreetNameProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<StreetNameCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(StreetNameCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();

            containerBuilder
                .RegisterType<MunicipalityCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(MunicipalityCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }

        public delegate CommandHandlerModule CommandHandlerModuleResolver(string key);

        public static void Register(IServiceCollection services)
        {
            services
                .AddSingleton<CrabStreetNameProvenanceFactory>()
                .AddSingleton<StreetNameLegacyProvenanceFactory>()
                .AddSingleton<StreetNameProvenanceFactory>();

            services.AddTransient<CommandHandlerModuleResolver>(serviceProvider => key =>
            {
                if (key == typeof(CrabStreetNameCommandHandlerModule).FullName)
                {
                    return serviceProvider.GetRequiredService<CrabStreetNameCommandHandlerModule>();
                }

                if (key == nameof(CommandHandlerModule))
                {
                    return serviceProvider.GetRequiredService<StreetNameCommandHandlerModule>();
                }

                if (key == typeof(StreetNameCommandHandlerModule).FullName)
                {
                    return serviceProvider.GetRequiredService<MunicipalityCommandHandlerModule>();
                }

                throw new KeyNotFoundException();
            });
        }
    }
}
