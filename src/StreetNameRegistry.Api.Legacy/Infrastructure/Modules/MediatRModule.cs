namespace StreetNameRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Postal;
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

            builder.Register(c => (IRequestHandler<ListRequest, StreetNameListResponse>)
                new ListHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<ConsumerPostalContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<DetailRequest, StreetNameResponse>)
                new DetailHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<CountRequest, TotaalAantalResponse>)
                new CountHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<ConsumerPostalContext>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<BosaStreetNameRequest, StreetNameBosaResponse>)
                new BosaHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();
        }
    }
}
