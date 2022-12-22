namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Municipality;
    using Municipality.Events;
    using Projections.BackOffice;
    using Xunit;

    public class StreetNameBackOfficeProjectionsTests : StreetNameBackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly TestBackOfficeContext _fakeBackOfficeContext;

        public StreetNameBackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());

            _fakeBackOfficeContext = new FakeBackOfficeContextFactory(true).CreateDbContext(Array.Empty<string>());
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
                .Given(streetNameWasProposedV2)
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
                CancellationToken.None);

            await Sut
                .Given(streetNameWasProposedV2)
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
        protected ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections> Sut { get; }
        protected Mock<IDbContextFactory<BackOfficeContext>> BackOfficeContextMock { get; }

        protected StreetNameBackOfficeProjectionsTest()
        {
            BackOfficeContextMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            Sut = new ConnectedProjectionTest<BackOfficeProjectionsContext, BackOfficeProjections>(
                CreateContext,
                () => new BackOfficeProjections(BackOfficeContextMock.Object));
        }

        protected virtual BackOfficeProjectionsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BackOfficeProjectionsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BackOfficeProjectionsContext(options);
        }
    }
}
