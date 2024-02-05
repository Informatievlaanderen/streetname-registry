namespace StreetNameRegistry.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using Generate;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Municipality;
    using Municipality.Events;
    using Projections.Integration;
    using Projections.Integration.Converters;
    using Projections.Integration.Infrastructure;
    using StreetName.Events;
    using Xunit;

    public class StreetNameVersionProjectionTests : IntegrationProjectionTest<StreetNameVersionProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/straatnaam";
        private readonly Fixture _fixture;
        private readonly Mock<IEventsRepository> _eventsRepositoryMock;

        public StreetNameVersionProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNameId());
            _fixture.Customize(new WithFixedMunicipalityId());

            _eventsRepositoryMock = new Mock<IEventsRepository>();
        }

         [Fact]
        public async Task WhenMunicipalityNisCodeWasChanged()
        {
            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(_fixture)
                .Build();

            var municipalityNisCodeWasChanged = _fixture.Create<MunicipalityNisCodeWasChanged>();

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };
            var secondMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1}
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, metadata)),
                    new Envelope<MunicipalityNisCodeWasChanged>(new Envelope(municipalityNisCodeWasChanged, secondMetadata)))
                .Then(async ct =>
                {
                    var streetNameVersion =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasMigratedToMunicipality.PersistentLocalId);
                    streetNameVersion.Should().NotBeNull();
                    streetNameVersion!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
                    streetNameVersion.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);
                    streetNameVersion.MunicipalityId.Should().Be(streetNameWasMigratedToMunicipality.MunicipalityId);

                    streetNameVersion.Namespace.Should().Be(Namespace);
                    streetNameVersion.Puri.Should().Be($"{Namespace}/{streetNameWasMigratedToMunicipality.PersistentLocalId}");
                    streetNameVersion.VersionTimestamp.Should().Be(municipalityNisCodeWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasMigratedToMunicipality()
        {
            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                })
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                    new StreetNameHomonymAddition("AZE", Language.English),
                    new StreetNameHomonymAddition("QSD", Language.German),
                }))
                .Build();

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position, streetNameWasMigratedToMunicipality.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
                    expectedLatestItem.OsloStatus.Should().Be(streetNameWasMigratedToMunicipality.Status.Map());
                    expectedLatestItem.NisCode.Should().Be(streetNameWasMigratedToMunicipality.NisCode);
                    expectedLatestItem.IsRemoved.Should().Be(streetNameWasMigratedToMunicipality.IsRemoved);
                    expectedLatestItem.MunicipalityId.Should().Be(streetNameWasMigratedToMunicipality.MunicipalityId);

                    expectedLatestItem.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.HomonymAdditionDutch.Should().Be("ABC");
                    expectedLatestItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedLatestItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedLatestItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasMigratedToMunicipality.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasMigratedToMunicipality.Provenance.Timestamp);
                    expectedLatestItem.CreatedOnTimestamp.Should().Be(streetNameWasMigratedToMunicipality.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposedV2()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                })
                .Build();

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be("Voorgesteld");
                    expectedLatestItem.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedLatestItem.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);

                    expectedLatestItem.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expectedLatestItem.CreatedOnTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasApproved = new StreetNameWasApprovedBuilder(_fixture)
                .Build();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be("InGebruik");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasCorrectedFromApprovedToProposed = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposed>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasCorrectedFromApprovedToProposed>(new Envelope(streetNameWasCorrectedFromApprovedToProposed,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be("Voorgesteld");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasRejected = _fixture.Create<StreetNameWasRejected>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasRejected>(new Envelope(streetNameWasRejected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be("Afgekeurd");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasCorrectedFromRejectedToProposed = _fixture.Create<StreetNameWasCorrectedFromRejectedToProposed>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasCorrectedFromRejectedToProposed>(new Envelope(streetNameWasCorrectedFromRejectedToProposed,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be("Voorgesteld");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasRetiredV2 = _fixture.Create<StreetNameWasRetiredV2>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasRetiredV2>(new Envelope(streetNameWasRetiredV2, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be("Gehistoreerd");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasRenamed = _fixture.Create<StreetNameWasRenamed>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasRenamed>(new Envelope(streetNameWasRenamed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be("Gehistoreerd");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasCorrectedFromRetiredToCurrent = _fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasCorrectedFromRetiredToCurrent>(new Envelope(streetNameWasCorrectedFromRetiredToCurrent,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.InGebruik.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrectedBuilder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                })
                .Build();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameNamesWereChanged = new StreetNameNamesWereChangedBuilder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                }).Build();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrectedBuilder(_fixture)
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                    new StreetNameHomonymAddition("AZE", Language.English),
                    new StreetNameHomonymAddition("QSD", Language.German),
                })).Build();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.HomonymAdditionDutch.Should().Be("ABC");
                    expectedLatestItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedLatestItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedLatestItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereRemovedBuilder(_fixture)
                .WithLanguages(new List<Language>()
                {
                    Language.Dutch,
                    Language.French,
                    Language.English,
                    Language.German,
                }).Build();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.HomonymAdditionDutch.Should().BeNull();
                    expectedLatestItem.HomonymAdditionFrench.Should().BeNull();
                    expectedLatestItem.HomonymAdditionEnglish.Should().BeNull();
                    expectedLatestItem.HomonymAdditionGerman.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2()
        {
            var streetNameWasProposedV2 = new StreetNameWasProposedV2Builder(_fixture)
                .Build();

            var streetNameWasRemovedV2 = _fixture.Create<StreetNameWasRemovedV2>();

            var position = _fixture.Create<long>();

            var firstEvenMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEvenMetadata)),
                    new Envelope<StreetNameWasRemovedV2>(new Envelope(streetNameWasRemovedV2, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FindAsync(position + 1, streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.IsRemoved.Should().BeTrue();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRemovedV2.Provenance.Timestamp);
                });
        }

        #region Legacy

        [Fact]
        public async Task WhenStreetNameWasRegistered()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, metadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions.FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);
                    expectedLatestItem.Position.Should().Be(position);
                    expectedLatestItem.MunicipalityId.Should().Be(streetNameWasRegistered.MunicipalityId);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRegistered.Provenance.Timestamp);
                    expectedLatestItem.CreatedOnTimestamp.Should().Be(streetNameWasRegistered.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasNamed()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasNamed = _fixture.Create<StreetNameWasNamed>()
                .WithLanguage(StreetName.Language.Dutch)
                .WithName("straat")
                .WithProvenance(_fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasNamed>(new Envelope(streetNameWasNamed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId && x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.NameDutch.Should().Be("straat");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasNamed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNameWasCorrected()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameNameWasCorrected = _fixture.Create<StreetNameNameWasCorrected>()
                .WithLanguage(StreetName.Language.Dutch)
                .WithName("straat")
                .WithProvenance(_fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameNameWasCorrected>(new Envelope(streetNameNameWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.NameDutch.Should().Be("straat");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNameWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNameWasCleared()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameNameWasCleared = _fixture.Create<StreetNameNameWasCleared>()
                .WithLanguage(StreetName.Language.Dutch)
                .WithProvenance(_fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameNameWasCleared>(new Envelope(streetNameNameWasCleared, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.NameDutch.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNameWasCleared.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNameWasCorrectedToCleared()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameNameWasCorrectedToCleared = _fixture.Create<StreetNameNameWasCorrectedToCleared>()
                .WithLanguage(StreetName.Language.Dutch)
                .WithProvenance(_fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameNameWasCorrectedToCleared>(new Envelope(streetNameNameWasCorrectedToCleared, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.NameDutch.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNameWasCorrectedToCleared.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionWasDefined()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameHomonymAdditionWasDefined = _fixture.Create<StreetNameHomonymAdditionWasDefined>()
                .WithHomonymAddition(new StreetName.StreetNameHomonymAddition("ABC", StreetName.Language.Dutch), _fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasDefined>(new Envelope(streetNameHomonymAdditionWasDefined, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.HomonymAdditionDutch.Should().Be("ABC");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionWasDefined.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionWasCorrected()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameHomonymAdditionWasCorrected = _fixture.Create<StreetNameHomonymAdditionWasCorrected>()
                .WithHomonymAddition(new StreetName.StreetNameHomonymAddition("ABC", StreetName.Language.Dutch), _fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasCorrected>(new Envelope(streetNameHomonymAdditionWasCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.HomonymAdditionDutch.Should().Be("ABC");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionWasCleared()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameHomonymAdditionWasDefined = _fixture.Create<StreetNameHomonymAdditionWasDefined>()
                .WithHomonymAddition(new StreetName.StreetNameHomonymAddition("ABC", StreetName.Language.Dutch), _fixture.Create<Provenance>());
            var streetNameHomonymAdditionWasCleared = _fixture.Create<StreetNameHomonymAdditionWasCleared>()
                .WithLanguage(StreetName.Language.Dutch, _fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            var thirdEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasDefined>(new Envelope(streetNameHomonymAdditionWasDefined, secondEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasCleared>(new Envelope(streetNameHomonymAdditionWasCleared, thirdEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 2);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.HomonymAdditionDutch.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionWasCleared.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionWasCorrectedToCleared()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameHomonymAdditionWasDefined = _fixture.Create<StreetNameHomonymAdditionWasDefined>()
                .WithHomonymAddition(new StreetName.StreetNameHomonymAddition("ABC", StreetName.Language.Dutch), _fixture.Create<Provenance>());
            var streetNameHomonymAdditionWasCorrectedToCleared = _fixture.Create<StreetNameHomonymAdditionWasCorrectedToCleared>()
                .WithLanguage(StreetName.Language.Dutch, _fixture.Create<Provenance>());

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            var thirdEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasDefined>(new Envelope(streetNameHomonymAdditionWasDefined, secondEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>(new Envelope(streetNameHomonymAdditionWasCorrectedToCleared,
                        thirdEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 2);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.HomonymAdditionDutch.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionWasCorrectedToCleared.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameBecameComplete()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameBecameComplete = _fixture.Create<StreetNameBecameComplete>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameBecameComplete>(new Envelope(streetNameBecameComplete, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameBecameComplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameBecameIncomplete()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameBecameIncomplete = _fixture.Create<StreetNameBecameIncomplete>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameBecameIncomplete>(new Envelope(streetNameBecameIncomplete, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameBecameIncomplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemoved()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasRemoved = _fixture.Create<StreetNameWasRemoved>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasRemoved>(new Envelope(streetNameWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);
                    expectedLatestItem.IsRemoved.Should().BeTrue();
                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNamePersistentLocalIdWasAssigned()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNamePersistentLocalIdWasAssigned = _fixture.Create<StreetNamePersistentLocalIdWasAssigned>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNamePersistentLocalIdWasAssigned>(new Envelope(streetNamePersistentLocalIdWasAssigned, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNamePersistentLocalIdWasAssigned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameBecameCurrent()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameBecameCurrent = _fixture.Create<StreetNameBecameCurrent>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameBecameCurrent>(new Envelope(streetNameBecameCurrent, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.InGebruik.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameBecameCurrent.Provenance.Timestamp);
                });
        }


        [Fact]
        public async Task WhenStreetNameWasProposed()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasProposed = _fixture.Create<StreetNameWasProposed>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasProposed>(new Envelope(streetNameWasProposed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Voorgesteld.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasProposed.Provenance.Timestamp);
                });
        }


        [Fact]
        public async Task WhenStreetNameWasStreetNameWasRetired()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasRetired = _fixture.Create<StreetNameWasRetired>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasRetired>(new Envelope(streetNameWasRetired, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Gehistoreerd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRetired.Provenance.Timestamp);
                });
        }


        [Fact]
        public async Task WhenStreetNameWasCorrectedToCurrent()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasCorrectedToCurrent = _fixture.Create<StreetNameWasCorrectedToCurrent>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedToCurrent>(new Envelope(streetNameWasCorrectedToCurrent, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.InGebruik.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedToCurrent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedToProposed()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasCorrectedToProposed = _fixture.Create<StreetNameWasCorrectedToProposed>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedToProposed>(new Envelope(streetNameWasCorrectedToProposed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Voorgesteld.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedToRetired()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameWasCorrectedToRetired = _fixture.Create<StreetNameWasCorrectedToRetired>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedToRetired>(new Envelope(streetNameWasCorrectedToRetired, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Gehistoreerd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedToRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameStatusWasRemoved()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameStatusWasRemoved = _fixture.Create<StreetNameStatusWasRemoved>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameStatusWasRemoved>(new Envelope(streetNameStatusWasRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().BeNull();
                    expectedLatestItem.OsloStatus.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameStatusWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameStatusWasCorrectedToRemoved()
        {
            var streetNameWasRegistered = _fixture.Create<StreetNameWasRegistered>();
            var streetNameStatusWasCorrectedToRemoved = _fixture.Create<StreetNameStatusWasCorrectedToRemoved>();

            var persistentLocalId = _fixture.Create<PersistentLocalId>();
            _eventsRepositoryMock.Setup(x => x.GetPersistentLocalId(streetNameWasRegistered.StreetNameId))
                .ReturnsAsync(persistentLocalId);

            var position = _fixture.Create<long>();

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            var secondEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasRegistered>(new Envelope(streetNameWasRegistered, firstEventMetadata)),
                    new Envelope<StreetNameStatusWasCorrectedToRemoved>(new Envelope(streetNameStatusWasCorrectedToRemoved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameVersions
                            .FirstAsync(x => x.StreetNameId == streetNameWasRegistered.StreetNameId &&
                                             x.Position == position + 1);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem.PersistentLocalId.Should().Be(persistentLocalId);
                    expectedLatestItem.NisCode.Should().Be(streetNameWasRegistered.NisCode);

                    expectedLatestItem.Status.Should().BeNull();
                    expectedLatestItem.OsloStatus.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{persistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameStatusWasCorrectedToRemoved.Provenance.Timestamp);
                });
        }

        #endregion

        protected override StreetNameVersionProjections CreateProjection()
            => new StreetNameVersionProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }), _eventsRepositoryMock.Object);
    }
}
