namespace StreetNameRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;
    using Projections.Syndication;
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
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<DetailRequest, StreetNameResponse>)
                new DetailHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<CountRequest, TotaalAantalResponse>)
                new CountHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<SyndicationContext>())).InstancePerLifetimeScope();
        }
    }
}
