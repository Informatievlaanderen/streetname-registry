namespace StreetNameRegistry.Infrastructure.Modules
{
    using AllStream;
    using Autofac;
    using Municipality;
    using StreetName;
    using Repositories;

    public class RepositoriesModule : Module
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

            builder
                .RegisterType<AllStreamRepository>()
                .As<IAllStreamRepository>();
        }
    }
}
