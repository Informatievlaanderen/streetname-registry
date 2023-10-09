namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Abstractions.Infrastructure.Options;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
    using FeatureToggles;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using StreetName.Count;
    using StreetName.Detail;
    using StreetName.List;
    using Module = Autofac.Module;
    using OsloCountHandlerV2 = StreetName.Count.OsloCountHandlerV2;
    using OsloListHandler = StreetName.List.OsloListHandler;
    using OsloListHandlerV2 = StreetName.List.OsloListHandlerV2;

    public sealed class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<OsloListRequest, StreetNameListOsloResponse>)
                        new OsloListHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<ConsumerPostalContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<OsloListRequest, StreetNameListOsloResponse>)
                    new OsloListHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<ConsumerPostalContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<OsloDetailRequest, StreetNameOsloResponse>)
                        new OsloDetailHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<OsloDetailRequest, StreetNameOsloResponse>)
                    new OsloDetailHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<OsloCountRequest, TotaalAantalResponse>)
                        new OsloCountHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<ConsumerPostalContext>());
                }

                return (IRequestHandler<OsloCountRequest, TotaalAantalResponse>)
                    new OsloCountHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<ConsumerPostalContext>());
            }).InstancePerLifetimeScope();
        }
    }
}
