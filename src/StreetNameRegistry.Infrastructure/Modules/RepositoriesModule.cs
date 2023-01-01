namespace StreetNameRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Municipality;
    using StreetName;
    using Repositories;

    public class RepositoriesModule : Module, IServiceCollectionModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            builder
                .RegisterType<StreetNames>()
                .As<IStreetNames>();

            builder
                .RegisterType<Municipalities>()
                .As<IMunicipalities>();
        }

        public void Load(IServiceCollection services)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            services
                .AddTransient<IStreetNames, StreetNames>()
                .AddTransient<IMunicipalities, Municipalities>();
        }
    }
}
