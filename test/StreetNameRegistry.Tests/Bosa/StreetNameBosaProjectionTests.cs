namespace StreetNameRegistry.Tests.Bosa
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using Projections.Bosa;
    using Projections.Bosa.Infrastructure;
    using Xunit;

    public class StreetNameBosaProjectionTests : BosaProjectionTest<StreetNameBosaProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/straatnaam";
        private readonly Fixture _fixture;

        public StreetNameBosaProjectionTests()
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasMigratedToMunicipality.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
                    expectedItem.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);
                    expectedItem.MunicipalityId.Should().Be(streetNameWasMigratedToMunicipality.MunicipalityId);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(municipalityNisCodeWasChanged.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasMigratedToMunicipality.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(streetNameWasMigratedToMunicipality.Status);
                    expectedItem.NisCode.Should().Be(streetNameWasMigratedToMunicipality.NisCode);
                    expectedItem.IsRemoved.Should().Be(streetNameWasMigratedToMunicipality.IsRemoved);
                    expectedItem.MunicipalityId.Should().Be(streetNameWasMigratedToMunicipality.MunicipalityId);

                    expectedItem.NameDutch.Should().Be("Bergstraat");
                    expectedItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedItem.NameEnglish.Should().Be("Mountain street");
                    expectedItem.NameGerman.Should().Be("Bergstraat de");

                    expectedItem.HomonymAdditionDutch.Should().Be("ABC");
                    expectedItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasMigratedToMunicipality.Provenance.Timestamp);
                    expectedItem.CreatedOn.Should().Be(streetNameWasMigratedToMunicipality.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposedForMunicipalityMerger()
        {
            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMergerBuilder(_fixture)
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

            var firstEventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<StreetNameWasProposedForMunicipalityMerger>(new Envelope(streetNameWasProposedForMunicipalityMerger, firstEventMetadata)))
                .Then(async ct =>
                {
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasProposedForMunicipalityMerger.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedItem.NisCode.Should().Be(streetNameWasProposedForMunicipalityMerger.NisCode);
                    expectedItem.MunicipalityId.Should().Be(streetNameWasProposedForMunicipalityMerger.MunicipalityId);

                    expectedItem.IsRemoved.Should().BeFalse();

                    expectedItem.NameDutch.Should().Be("Bergstraat");
                    expectedItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedItem.NameEnglish.Should().Be("Mountain street");
                    expectedItem.NameGerman.Should().Be("Bergstraat de");

                    expectedItem.HomonymAdditionDutch.Should().Be("ABC");
                    expectedItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp);
                    expectedItem.CreatedOn.Should().Be(streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasProposedV2.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Proposed);
                    expectedItem.NisCode.Should().Be(streetNameWasProposedV2.NisCode);
                    expectedItem.MunicipalityId.Should().Be(streetNameWasProposedV2.MunicipalityId);

                    expectedItem.IsRemoved.Should().BeFalse();

                    expectedItem.NameDutch.Should().Be("Bergstraat");
                    expectedItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedItem.NameEnglish.Should().Be("Mountain street");
                    expectedItem.NameGerman.Should().Be("Bergstraat de");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
                    expectedItem.CreatedOn.Should().Be(streetNameWasProposedV2.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasApproved.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Current);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasApproved.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Proposed);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasRejected.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Rejected);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasRejected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger()
        {
            var streetNameWasMigratedToMunicipality = _fixture.Create<StreetNameWasMigratedToMunicipality>();
            var streetNameWasRejectedBecauseOfMunicipalityMerger = _fixture.Create<StreetNameWasRejectedBecauseOfMunicipalityMerger>();

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
                    new Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>(new Envelope(streetNameWasRejectedBecauseOfMunicipalityMerger, secondEventMetadata)))
                .Then(async ct =>
                {
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasRejectedBecauseOfMunicipalityMerger.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Rejected);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Proposed);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasRetiredV2.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Retired);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasRetiredV2.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Retired);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasRenamed.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Retired);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasRenamed.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId);
                    expectedItem.Should().NotBeNull();
                    expectedItem!.Status.Should().Be(StreetNameStatus.Current);

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameNamesWereCorrected.PersistentLocalId);
                    expectedItem.Should().NotBeNull();

                    expectedItem!.NameDutch.Should().Be("Bergstraat");
                    expectedItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedItem.NameEnglish.Should().Be("Mountain street");
                    expectedItem.NameGerman.Should().Be("Bergstraat de");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameNamesWereCorrected.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameNamesWereChanged.PersistentLocalId);
                    expectedItem.Should().NotBeNull();

                    expectedItem!.NameDutch.Should().Be("Bergstraat");
                    expectedItem.NameFrench.Should().Be("Rue De Montaigne");
                    expectedItem.NameEnglish.Should().Be("Mountain street");
                    expectedItem.NameGerman.Should().Be("Bergstraat de");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameNamesWereChanged.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameHomonymAdditionsWereCorrected.PersistentLocalId);
                    expectedItem.Should().NotBeNull();

                    expectedItem!.HomonymAdditionDutch.Should().Be("ABC");
                    expectedItem.HomonymAdditionFrench.Should().Be("DEF");
                    expectedItem.HomonymAdditionEnglish.Should().Be("AZE");
                    expectedItem.HomonymAdditionGerman.Should().Be("QSD");

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
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
                    var expectedItem =
                        await ct.StreetNames.FindAsync(streetNameHomonymAdditionsWereCorrected.PersistentLocalId);
                    expectedItem.Should().NotBeNull();

                    expectedItem!.HomonymAdditionDutch.Should().BeNull();
                    expectedItem.HomonymAdditionFrench.Should().BeNull();
                    expectedItem.HomonymAdditionEnglish.Should().BeNull();
                    expectedItem.HomonymAdditionGerman.Should().BeNull();

                    expectedItem.Namespace.Should().Be(Namespace);
                    expectedItem.VersionTimestamp.Should().Be(streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp);
                });
        }


        protected override StreetNameBosaProjections CreateProjection()
            => new StreetNameBosaProjections(
                new OptionsWrapper<BosaOptions>(new BosaOptions { Namespace = Namespace }),
                new BosaDay(new BosaOptions { ChangesFrom = "2000-01-01T00:00:00+01:00" }));
    }
}
