namespace StreetNameRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using Municipality;
    using Municipality.Events;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Infrastructure;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class IfMatchValidatorTests : StreetNameRegistryTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public IfMatchValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task IfMatchHeaderValid_ShouldReturnTrue()
        {
            var municipalityId = new MunicipalityId(Guid.NewGuid());
            var nisCode = Fixture.Create<NisCode>();
            var streetNamePersistentLocalId = new PersistentLocalId(456);
            var streetNameWasProposedV2 = new StreetNameWasProposedV2(municipalityId, nisCode, Fixture.Create<Names>(), streetNamePersistentLocalId);
            ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            _backOfficeContext.AddMunicipalityIdByPersistentLocalIdToFixture(streetNamePersistentLocalId, municipalityId);
            var municipalities = new Mock<IMunicipalities>();
            municipalities.Setup(x => x.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None))
                .ReturnsAsync(() =>
                {
                    var municipality = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();

                    municipality.Initialize(new List<object>
                    {
                        new MunicipalityWasImported(municipalityId, nisCode),
                        streetNameWasProposedV2
                    });

                    return municipality;
                });

            var expectedEtag = new ETag(ETagType.Strong, streetNameWasProposedV2.GetHash());

            var sut = new IfMatchHeaderValidator(_backOfficeContext, municipalities.Object);

            // Act
            var result = await sut.IsValid(expectedEtag.ToString(), streetNamePersistentLocalId, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IfMatchHeaderNotValid_ShouldReturnFalse()
        {
            var municipalityId = new MunicipalityId(Guid.NewGuid());
            var streetNamePersistentLocalId = new PersistentLocalId(456);
            var nisCode = new NisCode("23002");

            var municipalities = new Mock<IMunicipalities>();
            municipalities.Setup(x => x.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None))
                .ReturnsAsync(() =>
                {
                    var municipality = new MunicipalityFactory(NoSnapshotStrategy.Instance).Create();
                    var streetNameWasProposedV2 = new StreetNameWasProposedV2(municipalityId, nisCode, Fixture.Create<Names>(), streetNamePersistentLocalId);
                    ((ISetProvenance)streetNameWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

                    municipality.Initialize(new List<object>
                    {
                        new MunicipalityWasImported(municipalityId, nisCode),
                        streetNameWasProposedV2
                    });

                    return municipality;
                });

            _backOfficeContext.AddMunicipalityIdByPersistentLocalIdToFixture(streetNamePersistentLocalId, municipalityId);

            var sut = new IfMatchHeaderValidator(_backOfficeContext, municipalities.Object);

            // Act
            var result = await sut.IsValid("NON MATCHING ETAG", streetNamePersistentLocalId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task WhenMunicipalityIdNotFoundForStreetName_ShouldThrowStreetNameNotFoundException()
        {
            var streetNamePersistentLocalId = new PersistentLocalId(456);
            var sut = new IfMatchHeaderValidator(_backOfficeContext, Container.Resolve<IMunicipalities>());

            // Act
            Func<Task> act = async() => await sut.IsValid(string.Empty, streetNamePersistentLocalId, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
