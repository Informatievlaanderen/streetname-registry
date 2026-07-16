namespace StreetNameRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using StreetName.V2;
    using StreetName.V2.Count;
    using StreetName.V2.Detail;
    using StreetName.V2.List;
    using StreetName.V2.Sync;
    using V3 = StreetName.V3;
    using V3Count = StreetName.V3.Count;
    using V3Detail = StreetName.V3.Detail;
    using V3List = StreetName.V3.List;

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
                        c.Resolve<IOptions<ResponseOptionsV2>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<OsloDetailRequest, StreetNameOsloResponse>)
                new OsloDetailHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptionsV2>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<OsloCountRequest, TotaalAantalResponse>)
                new ElasticOsloCountHandler(
                    c.Resolve<IStreetNameApiElasticSearchClient>()))
                .InstancePerLifetimeScope();

            builder.RegisterType<SyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3List.OsloListRequest, V3List.StreetNameListOsloV3Response>)
                    new V3List.ElasticOsloListHandler(
                        c.Resolve<V3.IStreetNameApiElasticSearchClient>(),
                        c.Resolve<IOptions<ResponseOptionsV3>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3Detail.OsloDetailRequest, V3Detail.StreetNameOsloV3Response>)
                new V3Detail.OsloDetailHandler(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptionsV3>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3Count.OsloCountRequest, Be.Vlaanderen.Basisregisters.GrAr.Oslo.TotaalAantalResponse>)
                new V3Count.ElasticOsloCountHandler(
                    c.Resolve<V3.IStreetNameApiElasticSearchClient>()))
                .InstancePerLifetimeScope();
        }
    }
}
