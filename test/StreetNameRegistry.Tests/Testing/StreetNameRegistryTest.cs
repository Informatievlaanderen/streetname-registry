namespace StreetNameRegistry.Tests.Testing
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class StreetNameRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }
        protected string ConfigDetailUrl => "http://base/{0}";

        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        protected StreetNameRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:Events", "x" },
                    { "ConnectionStrings:Snapshots", "x" },
                    { "DetailUrl", ConfigDetailUrl }
                })
                .Build();

            builder.Register((a) => (IConfiguration)configuration);

            builder
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule())
                .RegisterModule(new SqlSnapshotStoreModule());

            builder
                .Register(c => new MunicipalityFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IMunicipalityFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }

        protected string FormatDetailUrl(object o) => string.Format(ConfigDetailUrl, o);
    }
}
