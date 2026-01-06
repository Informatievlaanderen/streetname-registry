namespace StreetNameRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Municipality;
    using Municipality.Commands;
    using Municipality.DataStructures;
    using Municipality.Events;
    using Extensions;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class TakeMunicipalitySnapshotTests : StreetNameRegistryTest
    {
        public TakeMunicipalitySnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedPersistentLocalId());
        }

        [Fact]
        public void MunicipalityWasImportedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityWasImported = Fixture.Create<MunicipalityWasImported>();

            aggregate.Initialize(new List<object>
            {
                municipalityWasImported
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.NisCode.Should().Be(municipalityWasImported.NisCode);
            municipalitySnapshot.MunicipalityId.Should().Be(municipalityWasImported.MunicipalityId);
            municipalitySnapshot.MunicipalityStatus.Should().Be(MunicipalityStatus.Proposed);
            municipalitySnapshot.StreetNames.Should().BeEmpty();
            municipalitySnapshot.OfficialLanguages.Should().BeEmpty();
            municipalitySnapshot.FacilityLanguages.Should().BeEmpty();
            municipalitySnapshot.IsRemoved.Should().BeFalse();
        }

        [Fact]
        public void MunicipalityNisCodeWasChangedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityNisCodeWasChanged = Fixture.Create<MunicipalityNisCodeWasChanged>();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityNisCodeWasChanged
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.NisCode.Should().Be(municipalityNisCodeWasChanged.NisCode);
        }

        [Fact]
        public void MunicipalityBecameCurrentIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.MunicipalityStatus.Should().Be(MunicipalityStatus.Current);
        }

        [Fact]
        public void MunicipalityWasCorrectedToCurrentIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityWasRetired>(),
                Fixture.Create<MunicipalityWasCorrectedToCurrent>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.MunicipalityStatus.Should().Be(MunicipalityStatus.Current);
        }

        [Fact]
        public void MunicipalityWasRetiredIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityWasRetired>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.MunicipalityStatus.Should().Be(MunicipalityStatus.Retired);
        }

        [Fact]
        public void MunicipalityWasCorrectedToRetiredIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                Fixture.Create<MunicipalityBecameCurrent>(),
                Fixture.Create<MunicipalityWasCorrectedToRetired>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.MunicipalityStatus.Should().Be(MunicipalityStatus.Retired);
        }

        [Fact]
        public void MunicipalityOfficialLanguageWasAddedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityDutchOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.Dutch).ToEvent();
            var municipalityFrenchOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.French).ToEvent();
            var municipalityGermanOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.German).ToEvent();
            var municipalityEnglishOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.English).ToEvent();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityDutchOfficialLanguageWasAdded,
                municipalityFrenchOfficialLanguageWasAdded,
                municipalityGermanOfficialLanguageWasAdded,
                municipalityEnglishOfficialLanguageWasAdded
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.OfficialLanguages.Should().BeEquivalentTo(new List<Language>
            {
                Language.Dutch, Language.French, Language.German, Language.English
            });
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.German)]
        [InlineData(Language.English)]
        public void MunicipalityOfficialLanguageWasRemovedIsSavedInSnapshot(Language languageToRemove)
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityDutchOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.Dutch).ToEvent();
            var municipalityFrenchOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.French).ToEvent();
            var municipalityGermanOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.German).ToEvent();
            var municipalityEnglishOfficialLanguageWasAdded = Fixture.Create<AddOfficialLanguageToMunicipality>().WithLanguage(Language.English).ToEvent();
            var municipalityOfficialLanguageWasRemoved = Fixture.Create<RemoveOfficialLanguageFromMunicipality>().WithLanguage(languageToRemove).ToEvent();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityDutchOfficialLanguageWasAdded,
                municipalityFrenchOfficialLanguageWasAdded,
                municipalityGermanOfficialLanguageWasAdded,
                municipalityEnglishOfficialLanguageWasAdded,
                municipalityOfficialLanguageWasRemoved
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            var expectedLanguages = new List<Language>
            {
                Language.Dutch, Language.French, Language.German, Language.English
            };
            expectedLanguages.Remove(languageToRemove);
            municipalitySnapshot.OfficialLanguages.Should().BeEquivalentTo(expectedLanguages);
        }

        [Fact]
        public void MunicipalityFacilityLanguageWasAddedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityDutchFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.Dutch).ToEvent();
            var municipalityFrenchFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.French).ToEvent();
            var municipalityGermanFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.German).ToEvent();
            var municipalityEnglishFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.English).ToEvent();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityDutchFacilityLanguageWasAdded,
                municipalityFrenchFacilityLanguageWasAdded,
                municipalityGermanFacilityLanguageWasAdded,
                municipalityEnglishFacilityLanguageWasAdded
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.FacilityLanguages.Should().BeEquivalentTo(new List<Language>
            {
                Language.Dutch, Language.French, Language.German, Language.English
            });
        }

        [Theory]
        [InlineData(Language.Dutch)]
        [InlineData(Language.French)]
        [InlineData(Language.German)]
        [InlineData(Language.English)]
        public void MunicipalityFacilityLanguageWasRemovedIsSavedInSnapshot(Language languageToRemove)
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityDutchFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.Dutch).ToEvent();
            var municipalityFrenchFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.French).ToEvent();
            var municipalityGermanFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.German).ToEvent();
            var municipalityEnglishFacilityLanguageWasAdded = Fixture.Create<AddFacilityLanguageToMunicipality>().WithLanguage(Language.English).ToEvent();
            var municipalityFacilityLanguageWasRemoved = Fixture.Create<RemoveFacilityLanguageFromMunicipality>().WithLanguage(languageToRemove).ToEvent();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityDutchFacilityLanguageWasAdded,
                municipalityFrenchFacilityLanguageWasAdded,
                municipalityGermanFacilityLanguageWasAdded,
                municipalityEnglishFacilityLanguageWasAdded,
                municipalityFacilityLanguageWasRemoved
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            var expectedLanguages = new List<Language>
            {
                Language.Dutch, Language.French, Language.German, Language.English
            };
            expectedLanguages.Remove(languageToRemove);
            municipalitySnapshot.FacilityLanguages.Should().BeEquivalentTo(expectedLanguages);
        }

        [Fact]
        public void StreetNameWasMigratedToMunicipalityIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasMigratedToMunicipality = Fixture.Create<StreetNameWasMigratedToMunicipality>();
            ((ISetProvenance)streetNameWasMigratedToMunicipality).SetProvenance(Fixture.Create<Provenance>());
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasMigratedToMunicipality
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasMigratedToMunicipality.PersistentLocalId),
                    streetNameWasMigratedToMunicipality.Status,
                    new Names(streetNameWasMigratedToMunicipality.Names),
                    new HomonymAdditions(streetNameWasMigratedToMunicipality.HomonymAdditions),
                    streetNameWasMigratedToMunicipality.IsRemoved,
                    false,
                    new StreetNameId(streetNameWasMigratedToMunicipality.StreetNameId),
                    [],
                    null,
                    streetNameWasMigratedToMunicipality.GetHash(),
                    streetNameWasMigratedToMunicipality.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasProposedV2IsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            streetNameWasProposedV2.SetProvenance(Fixture.Create<Provenance>());
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasProposedV2.PersistentLocalId),
                    StreetNameStatus.Proposed,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasProposedV2.GetHash(),
                    streetNameWasProposedV2.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasApprovedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            ((ISetProvenance)streetNameWasApproved).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasApproved.PersistentLocalId),
                    StreetNameStatus.Current,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasApproved.GetHash(),
                    streetNameWasApproved.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRejectedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejected = Fixture.Create<StreetNameWasRejected>();
            ((ISetProvenance)streetNameWasRejected).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasRejected
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRejected.PersistentLocalId),
                    StreetNameStatus.Rejected,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasRejected.GetHash(),
                    streetNameWasRejected.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRejectedBecauseOfMunicipalityMergerIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasRejectedBecauseOfMunicipalityMerger = Fixture.Create<StreetNameWasRejectedBecauseOfMunicipalityMerger>();
            ((ISetProvenance)streetNameWasRejectedBecauseOfMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasRejectedBecauseOfMunicipalityMerger
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRejectedBecauseOfMunicipalityMerger.PersistentLocalId),
                    StreetNameStatus.Rejected,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasRejectedBecauseOfMunicipalityMerger.GetHash(),
                    streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRetiredIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredV2 = Fixture.Create<StreetNameWasRetiredV2>();
            ((ISetProvenance)streetNameWasRetiredV2).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameWasRetiredV2
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRetiredV2.PersistentLocalId),
                    StreetNameStatus.Retired,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasRetiredV2.GetHash(),
                    streetNameWasRetiredV2.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRetiredBecauseOfMunicipalityMergerIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredBecauseOfMunicipalityMerger = Fixture.Create<StreetNameWasRetiredBecauseOfMunicipalityMerger>();
            ((ISetProvenance)streetNameWasRetiredBecauseOfMunicipalityMerger).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameWasRetiredBecauseOfMunicipalityMerger
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId),
                    StreetNameStatus.Retired,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasRetiredBecauseOfMunicipalityMerger.GetHash(),
                    streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance)
            });
        }

        [Fact]
        public void StreetNameNamesWereCorrectedIsSavedInSnapshot()
        {
            var names = new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) });
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(), names);
            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameNamesWereCorrected
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameNamesWereCorrected.PersistentLocalId),
                    StreetNameStatus.Current,
                    names,
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameNamesWereCorrected.GetHash(),
                    streetNameNamesWereCorrected.Provenance)
            });
        }

        [Fact]
        public void StreetNameNamesWereChangedIsSavedInSnapshot()
        {
            var names = new Names(new[] { new StreetNameName("Kapelstraat", Language.Dutch) });
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameNamesWereChanged = new StreetNameNamesWereChanged(
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<PersistentLocalId>(), names);
            ((ISetProvenance)streetNameNamesWereChanged).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameNamesWereChanged
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameNamesWereChanged.PersistentLocalId),
                    StreetNameStatus.Current,
                    names,
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    streetNameNamesWereChanged.GetHash(),
                    streetNameNamesWereChanged.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasCorrectedFromRetiredToCurrentIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameWasRetiredV2 = Fixture.Create<StreetNameWasRetiredV2>();
            var nameWasCorrectedFromRetiredToCurrent = Fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();
            ((ISetProvenance)nameWasCorrectedFromRetiredToCurrent).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameWasRetiredV2,
                nameWasCorrectedFromRetiredToCurrent
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(nameWasCorrectedFromRetiredToCurrent.PersistentLocalId),
                    StreetNameStatus.Current,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    false,
                    null,
                    [],
                    null,
                    nameWasCorrectedFromRetiredToCurrent.GetHash(),
                    nameWasCorrectedFromRetiredToCurrent.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRemovedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameWasRemovedV2 = Fixture.Create<StreetNameWasRemovedV2>();
            ((ISetProvenance)streetNameWasRemovedV2).SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameWasRemovedV2
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRemovedV2.PersistentLocalId),
                    StreetNameStatus.Current,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    true,
                    false,
                    null,
                    [],
                    null,
                    streetNameWasRemovedV2.GetHash(),
                    streetNameWasRemovedV2.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasRenamedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var streetNameWasProposedV2 = Fixture.Create<StreetNameWasProposedV2>();
            var streetNameWasApproved = Fixture.Create<StreetNameWasApproved>();
            var streetNameWasRenamed = Fixture.Create<StreetNameWasRenamed>();
            streetNameWasRenamed.SetProvenance(Fixture.Create<Provenance>());

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedV2,
                streetNameWasApproved,
                streetNameWasRenamed
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasRenamed.PersistentLocalId),
                    StreetNameStatus.Retired,
                    new Names(streetNameWasProposedV2.StreetNameNames),
                    new HomonymAdditions(),
                    false,
                    true,
                    null,
                    [],
                    null,
                    streetNameWasRenamed.GetHash(),
                    streetNameWasRenamed.Provenance)
            });
        }

        [Fact]
        public void StreetNameWasProposedForMunicipalityMergerIsSavedInSnapshot()
        {
            var streetNameName = Fixture.Create<StreetNameName>();
            var homonym = new StreetNameHomonymAddition(new string(Fixture.CreateMany<char>(5).ToArray()), Language.Dutch);
            Fixture.Register(() => new Names { streetNameName });
            Fixture.Register(() => new HomonymAdditions { homonym });
            Fixture.Register(() => Language.Dutch);

            var streetNameWasProposedForMunicipalityMerger = Fixture.Create<StreetNameWasProposedForMunicipalityMerger>();

            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                streetNameWasProposedForMunicipalityMerger
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.StreetNames.Should().BeEquivalentTo(new List<StreetNameData>
            {
                new StreetNameData(
                    new PersistentLocalId(streetNameWasProposedForMunicipalityMerger.PersistentLocalId),
                    StreetNameStatus.Proposed,
                    new Names(streetNameWasProposedForMunicipalityMerger.StreetNameNames),
                    new HomonymAdditions(streetNameWasProposedForMunicipalityMerger.HomonymAdditions),
                    isRemoved:false,
                    isRenamed:false,
                    legacyStreetNameId:null,
                    streetNameWasProposedForMunicipalityMerger.MergedStreetNamePersistentLocalIds.Select(x => new PersistentLocalId(x)).ToList(),
                    streetNameWasProposedForMunicipalityMerger.DesiredStatus,
                    streetNameWasProposedForMunicipalityMerger.GetHash(),
                    streetNameWasProposedForMunicipalityMerger.Provenance)
            });
        }

        [Fact]
        public void MunicipalityWasRemovedIsSavedInSnapshot()
        {
            var aggregate = new MunicipalityFactory(IntervalStrategy.Default).Create();

            var municipalityWasRemoved = Fixture.Create<MunicipalityWasRemoved>();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MunicipalityWasImported>(),
                municipalityWasRemoved
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<MunicipalitySnapshot>();
            var municipalitySnapshot = (MunicipalitySnapshot)snapshot;

            municipalitySnapshot.IsRemoved.Should().BeTrue();
        }
    }
}
