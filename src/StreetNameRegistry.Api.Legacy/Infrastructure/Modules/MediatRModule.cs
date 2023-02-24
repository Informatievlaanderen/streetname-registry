namespace StreetNameRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FeatureToggles;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using StreetName.Bosa;
    using StreetName.Count;
    using StreetName.Detail;
    using StreetName.List;
    using StreetName.Sync;
    using Module = Autofac.Module;

    public sealed class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<ListRequest, StreetNameListResponse>)
                        new ListHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<ListRequest, StreetNameListResponse>)
                    new ListHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<DetailRequest, StreetNameResponse>)
                        new DetailHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<DetailRequest, StreetNameResponse>)
                    new DetailHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<CountRequest, TotaalAantalResponse>)
                        new CountHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>());
                }

                return (IRequestHandler<CountRequest, TotaalAantalResponse>)
                    new CountHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<BosaStreetNameRequest, StreetNameBosaResponse>)
                        new BosaHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<SyndicationContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<BosaStreetNameRequest, StreetNameBosaResponse>)
                    new BosaHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();
        }
    }
}
