namespace StreetNameRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Municipality;
    using StreetNameRegistry.Municipality.Events;
    using StreetNameRegistry.Projections.BackOffice;
    using StreetNameRegistry.Tests.BackOffice;
    using Xunit;

    public class StreetNameBackOfficeProjectionsTests : StreetNameBackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly TestBackOfficeContext _fakeBackOfficeContext;

        public StreetNameBackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());

            _fakeBackOfficeContext = new FakeBackOfficeContextFactory(true).CreateDbContext([]);
            BackOfficeContextMock
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeBackOfficeContext);
        }

        [Fact]
        public async Task GivenStreetNameWasProposed_ThenRelationIsAdded()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            await Sut
                .Given(BuildEnvelope(streetNameWasProposedV2))
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.MunicipalityIdByPersistentLocalId.FindAsync(streetNameWasProposedV2
                        .PersistentLocalId);

                    result.Should().NotBeNull();
                    result!.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);
                });
        }

        [Fact]
        public async Task GivenStreetNameWasProposedAndRelationPresent_ThenNothing()
        {
            _fixture.Register(() => new Names(_fixture.CreateMany<StreetNameName>(2).ToList()));
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentMunicipalityStreetNameIdRelation(
                streetNameWasProposedV2.PersistentLocalId,
                streetNameWasProposedV2.MunicipalityId,
                streetNameWasProposedV2.NisCode,
                CancellationToken.None);

            await Sut
                .Given(BuildEnvelope(streetNameWasProposedV2))
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.MunicipalityIdByPersistentLocalId.FindAsync(streetNameWasProposedV2
                        .PersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }
    }

    public abstract class StreetNameBackOfficeProjectionsTest
    {
        protected const int DelayInSeconds = 1;
        protected ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections> Sut { get; }
        protected Mock<IDbContextFactory<BackOfficeContext>> BackOfficeContextMock { get; }

        protected StreetNameBackOfficeProjectionsTest()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {nameof(DelayInSeconds), DelayInSeconds.ToString()}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            BackOfficeContextMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            Sut = new ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections>(
                CreateContext,
                () => new BackOfficeProjections(BackOfficeContextMock.Object, configuration));
        }

        protected virtual BackOfficeProjectionsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BackOfficeProjectionsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BackOfficeProjectionsContext(options);
        }

        protected Envelope<TMessage> BuildEnvelope<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            return new Envelope<TMessage>(new Envelope(message, new Dictionary<string, object>
            {
                { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
            }));
        }
    }
}
