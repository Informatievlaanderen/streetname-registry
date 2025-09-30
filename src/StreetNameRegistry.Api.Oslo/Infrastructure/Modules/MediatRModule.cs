namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Abstractions.Infrastructure.Options;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using StreetName;
    using StreetName.Count;
    using StreetName.Detail;
    using StreetName.List;
    using StreetName.Sync;

    public sealed class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<OsloListRequest, StreetNameListOsloResponse>)
                    new ElasticOsloListHandler(
                        c.Resolve<IStreetNameApiElasticSearchClient>(),
                        c.Resolve<IOptions<ResponseOptions>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<OsloDetailRequest, StreetNameOsloResponse>)
                new OsloDetailHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<OsloCountRequest, TotaalAantalResponse>)
                new ElasticOsloCountHandler(
                    c.Resolve<IStreetNameApiElasticSearchClient>()))
                .InstancePerLifetimeScope();

            builder.RegisterType<SyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
