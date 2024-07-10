namespace StreetNameRegistry.Tests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Xunit;

    public class StreetNameLatestItemProjectionTests : IntegrationProjectionTest<StreetNameLatestItemProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/straatnaam";
        private readonly Fixture _fixture;

        public StreetNameLatestItemProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedPersistentLocalId());
            _fixture.Customize(new WithFixedMunicipalityId());
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
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, metadata)),
                    new Envelope<MunicipalityNisCodeWasChanged>(new Envelope(municipalityNisCodeWasChanged, secondMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasMigratedToMunicipality.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
                    expectedLatestItem.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);
                    expectedLatestItem.MunicipalityId.Should().Be(streetNameWasMigratedToMunicipality.MunicipalityId);

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasMigratedToMunicipality.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(municipalityNisCodeWasChanged.Provenance.Timestamp);
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
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasMigratedToMunicipality.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
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

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };


            await Sut
                .Given(new Envelope<StreetNameWasProposedV2>(new Envelope(streetNameWasProposedV2, firstEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Voorgesteld.ToString());
                    expectedLatestItem.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedLatestItem.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);

                    expectedLatestItem.IsRemoved.Should().BeFalse();

                    expectedLatestItem.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasProposedV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasApproved>(new Envelope(streetNameWasApproved, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasApproved.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.InGebruik.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasApproved.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasCorrectedFromApprovedToProposed = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposed>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedFromApprovedToProposed>(new Envelope(streetNameWasCorrectedFromApprovedToProposed,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Voorgesteld.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasRejected = _fixture.Create<StreetNameWasRejected>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasRejected>(new Envelope(streetNameWasRejected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasRejected.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Rejected);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Afgekeurd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasRejected.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasCorrectedFromRejectedToProposed = _fixture.Create<StreetNameWasCorrectedFromRejectedToProposed>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedFromRejectedToProposed>(new Envelope(streetNameWasCorrectedFromRejectedToProposed,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Voorgesteld.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasRetiredV2 = _fixture.Create<StreetNameWasRetiredV2>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasRetiredV2>(new Envelope(streetNameWasRetiredV2, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasRetiredV2.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Gehistoreerd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasRetiredV2.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasRetiredBecauseOfMunicipalityMerger = _fixture.Create<StreetNameWasRetiredBecauseOfMunicipalityMerger>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>(new Envelope(streetNameWasRetiredBecauseOfMunicipalityMerger, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Gehistoreerd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasRenamed = _fixture.Create<StreetNameWasRenamed>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasRenamed>(new Envelope(streetNameWasRenamed, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasRenamed.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Retired);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.Gehistoreerd.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasRenamed.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasCorrectedFromRetiredToCurrent = _fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameWasCorrectedFromRetiredToCurrent>(new Envelope(streetNameWasCorrectedFromRetiredToCurrent,
                        secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();
                    expectedLatestItem!.Status.Should().Be(StreetNameStatus.Current);
                    expectedLatestItem.OsloStatus.Should().Be(StraatnaamStatus.InGebruik.ToString());

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameNamesWereCorrected>(new Envelope(streetNameNamesWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameNamesWereCorrected.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameNamesWereCorrected.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameNamesWereChanged = new StreetNameNamesWereChangedBuilder(_fixture)
                .WithNames(new Names
                {
                    new("Bergstraat", Language.Dutch),
                    new("Rue De Montaigne", Language.French),
                    new("Mountain street", Language.English),
                    new("Bergstraat de", Language.German),
                })
                .Build();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameNamesWereChanged>(new Envelope(streetNameNamesWereChanged, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameNamesWereChanged.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.NameDutch.Should().Be("Bergstraat");
                    expectedLatestItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedLatestItem.NameEnglish.Should().Be("Mountain street");
                    expectedLatestItem.NameGerman.Should().Be("Bergstraat de");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameNamesWereChanged.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrectedBuilder(_fixture)
                .WithHomonymAdditions(new HomonymAdditions(new[]
                {
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("DEF", Language.French),
                    new StreetNameHomonymAddition("AZE", Language.English),
                    new StreetNameHomonymAddition("QSD", Language.German),
                }))
                .Build();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereCorrected>(new Envelope(streetNameHomonymAdditionsWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameHomonymAdditionsWereCorrected.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.HomonymAdditionDutch.Should().Be("ABC");
                    expectedLatestItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedLatestItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedLatestItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameHomonymAdditionsWereCorrected.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved()
        {
            var streetNameWasMigratedToMunicipality = new StreetNameWasMigratedToMunicipalityBuilder(_fixture)
                    .WithHomonymAdditions(new HomonymAdditions(new[]
                    {
                        new StreetNameHomonymAddition("ABC", Language.Dutch),
                        new StreetNameHomonymAddition("DEF", Language.French),
                        new StreetNameHomonymAddition("AZE", Language.English),
                        new StreetNameHomonymAddition("QSD", Language.German),
                    }))
                    .Build();
            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereRemovedBuilder(_fixture)
                .WithLanguages(new List<Language>
                {
                    Language.Dutch,
                    Language.French,
                    Language.English,
                    Language.German
                })
                .Build();

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
                    new Envelope<StreetNameWasMigratedToMunicipality>(new Envelope(streetNameWasMigratedToMunicipality, firstEventMetadata)),
                    new Envelope<StreetNameHomonymAdditionsWereRemoved>(new Envelope(streetNameHomonymAdditionsWereCorrected, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedLatestItem =
                        await ct.StreetNameLatestItems.FindAsync(streetNameHomonymAdditionsWereCorrected.PersistentLocalId);
                    expectedLatestItem.Should().NotBeNull();

                    expectedLatestItem!.HomonymAdditionDutch.Should().BeNull();
                    expectedLatestItem.HomonymAdditionFrench.Should().BeNull();
                    expectedLatestItem.HomonymAdditionEnglish.Should().BeNull();
                    expectedLatestItem.HomonymAdditionGerman.Should().BeNull();

                    expectedLatestItem.Namespace.Should().Be(Namespace);
                    expectedLatestItem.Puri.Should().Be($"{Namespace}/{streetNameHomonymAdditionsWereCorrected.PersistentLocalId}");
                    expectedLatestItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
        }


        protected override StreetNameLatestItemProjections CreateProjection()
            => new StreetNameLatestItemProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
