namespace StreetNameRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.Testing.Infrastructure.Events;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Municipality.Events;
    using Newtonsoft.Json;
    using Producer;
    using Producer.Snapshot.Oslo;
    using Projections.Extract;
    using Projections.Extract.StreetNameExtract;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.LastChangedList;
    using Projections.Legacy;
    using Projections.Legacy.StreetNameDetailV2;
    using Projections.Legacy.StreetNameListV2;
    using Projections.Legacy.StreetNameSyndication;
    using Projections.Wfs;
    using Projections.Wfs.StreetNameHelperV2;
    using Projections.Wms;
    using SqlStreamStore;
    using Xunit;
    using ProducerContext = Producer.Snapshot.Oslo.ProducerContext;

    public class ProjectionsHandlesEventsTests
    {
        private readonly IEnumerable<Type> _eventsToExclude = new[] { typeof(MunicipalitySnapshot) };
        private readonly IEnumerable<Type> _eventTypes;

        public ProjectionsHandlesEventsTests()
        {
            _eventTypes = DiscoverEventTypes();
        }

        private IEnumerable<Type> DiscoverEventTypes()
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => InfrastructureEventsTests.GetAssemblyTypesSafe(a)
                .Any(t => t.Name == "DomainAssemblyMarker"));

            if (domainAssembly == null)
            {
                return Enumerable.Empty<Type>();
            }

            return domainAssembly.GetTypes()
                .Where(t => t is { IsClass: true, Namespace: not null } && IsEventNamespace(t) && IsNotCompilerGenerated(t))
                .Except(_eventsToExclude);
        }

        private static bool IsEventNamespace(Type t) => t.Namespace?.EndsWith("Municipality.Events") ?? false;
        private static bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

        [Theory]
        [MemberData(nameof(GetProjectionsToTest))]
        public void ProjectionsHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest)
        {
            AssertHandleEvents(projectionsToTest);
        }

        public static IEnumerable<object[]> GetProjectionsToTest()
        {
            yield return [new List<ConnectedProjection<LegacyContext>>
            {
                new StreetNameDetailProjectionsV2(),
                new StreetNameListProjectionsV2(),
                new StreetNameSyndicationProjections()
            }];

            yield return [new List<ConnectedProjection<WfsContext>>
            {
                new StreetNameHelperV2Projections()
            }];

            yield return [new List<ConnectedProjection<WmsContext>>
            {
                new StreetNameRegistry.Projections.Wms.StreetNameHelperV2.StreetNameHelperV2Projections()
            }];

            yield return [new List<ConnectedProjection<LastChangedListContext>>
            {
                new LastChangedProjections(Mock.Of<ICacheValidator>())
            }];

            yield return [new List<ConnectedProjection<IntegrationContext>>
            {
                new StreetNameLatestItemProjections(Mock.Of<IOptions<IntegrationOptions>>()), new StreetNameVersionProjections(Mock.Of<IOptions<IntegrationOptions>>(), Mock.Of<IEventsRepository>())
            }];

            yield return [new List<ConnectedProjection<ExtractContext>>
            {
                new StreetNameExtractProjectionsV2(Mock.Of<IReadonlyStreamStore>(), new EventDeserializer((_, _) => new object()), new OptionsWrapper<ExtractConfig>(new ExtractConfig()), Encoding.UTF8)
            }];

            yield return [new List<ConnectedProjection<ProducerContext>>
            {
                new ProducerProjections(Mock.Of<IProducer>(), Mock.Of<ISnapshotManager>(), "", Mock.Of<IOsloProxy>())
            }];

            yield return [new List<ConnectedProjection<StreetNameRegistry.Producer.ProducerContext>>
            {
                new ProducerMigrateProjections(Mock.Of<IProducer>())
            }];

            yield return [new List<ConnectedProjection<StreetNameRegistry.Producer.Ldes.ProducerContext>>
            {
                new StreetNameRegistry.Producer.Ldes.ProducerProjections(Mock.Of<IProducer>(), "http://s", new JsonSerializerSettings().ConfigureDefaultForApi())
            }];
        }

        private void AssertHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest)
        {
            foreach (var projection in projectionsToTest)
            {
                projection.Handlers.Should().NotBeEmpty();

                var handledEventTypes = projection.Handlers.Select(x => x.Message.GetGenericArguments().First()).ToList();
                var duplicateHandledEventTypes = handledEventTypes.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key).ToList();
                duplicateHandledEventTypes.Should().BeEmpty($"The projection {projection.GetType().Name} has duplicate event handlers for the events: {string.Join(", ", duplicateHandledEventTypes.Select(x => x.Name))}");

                foreach (var eventType in _eventTypes)
                {
                    var messageType = projection.Handlers.Any(x => x.Message.GetGenericArguments().First() == eventType);
                    messageType.Should().BeTrue($"The event {eventType.Name} is not handled by the projection {projection.GetType().Name}");
                }
            }
        }
    }
}
