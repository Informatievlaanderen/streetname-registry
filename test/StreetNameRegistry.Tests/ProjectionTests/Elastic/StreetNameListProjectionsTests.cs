namespace StreetNameRegistry.Tests.ProjectionTests.Elastic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Builders;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Municipality;
    using Municipality.Events;
    using Projections.Elastic;
    using Projections.Elastic.StreetNameList;
    using Projections.Syndication;
    using Tests.BackOffice;
    using Xunit;
    using Municipality = Projections.Elastic.StreetNameList.Municipality;

    public class StreetNameListProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly ConnectedProjectionTest<ElasticRunnerContext, StreetNameListProjections> _sut;

        private readonly Mock<IStreetNameListElasticClient> _elasticSearchClient;
        private readonly TestSyndicationContext _syndicationContext;
        private readonly Mock<IDbContextFactory<SyndicationContext>> _syndicationContextFactory;

        public StreetNameListProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());

            _elasticSearchClient = new Mock<IStreetNameListElasticClient>();
            _syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();
            _syndicationContextFactory = new Mock<IDbContextFactory<SyndicationContext>>();
            _syndicationContextFactory
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_syndicationContext);

            _sut = new ConnectedProjectionTest<ElasticRunnerContext, StreetNameListProjections>(CreateContext, CreateProjection);
        }

        private ElasticRunnerContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ElasticRunnerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ElasticRunnerContext(options);
        }

        private StreetNameListProjections CreateProjection() => new(
            _elasticSearchClient.Object,
            _syndicationContextFactory.Object);

        [Fact]
        public async Task WhenStreetNameWasProposedForMunicipalityMerger_ThenStreetNameWasAdded()
        {
            var streetNameWasProposedForMunicipalityMerger = new StreetNameWasProposedForMunicipalityMergerBuilder(_fixture)
                .WithHomonymAdditions(new HomonymAdditions([
                    new StreetNameHomonymAddition("ABC", Language.Dutch),
                    new StreetNameHomonymAddition("XYZ", Language.French)
                ]))
                .Build();

            await CreateMunicipality(streetNameWasProposedForMunicipalityMerger.NisCode);

            await _sut
                .Given(
                    streetNameWasProposedForMunicipalityMerger)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                                doc.StreetNamePersistentLocalId == streetNameWasProposedForMunicipalityMerger.PersistentLocalId
                                && doc.VersionTimestamp == streetNameWasProposedForMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset()
                                && doc.Status == StreetNameStatus.Proposed
                                && doc.Municipality.NisCode == streetNameWasProposedForMunicipalityMerger.NisCode
                                && Equals(doc.Names, streetNameWasProposedForMunicipalityMerger.StreetNameNames)
                                && Equals(doc.HomonymAdditions, streetNameWasProposedForMunicipalityMerger.HomonymAdditions)
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasProposedV2_ThenStreetNameWasAdded()
        {
            var streetNameWasProposedV2 = _fixture.Create<StreetNameWasProposedV2>();

            await CreateMunicipality(streetNameWasProposedV2.NisCode);

            await _sut
                .Given(streetNameWasProposedV2)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.CreateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                            doc.StreetNamePersistentLocalId == streetNameWasProposedV2.PersistentLocalId
                            && doc.VersionTimestamp == streetNameWasProposedV2.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Proposed
                            && doc.Municipality.NisCode == streetNameWasProposedV2.NisCode
                            && Equals(doc.Names, streetNameWasProposedV2.StreetNameNames)
                            && doc.HomonymAdditions.Length == 0
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasApproved_ThenStreetNameStatusWasChangedToApproved()
        {
            var streetNameWasApproved = _fixture.Create<StreetNameWasApproved>();

            await _sut
                .Given(streetNameWasApproved)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasApproved.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasApproved.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Current
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromApprovedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            var streetNameWasCorrectedFromApprovedToProposed = _fixture.Create<StreetNameWasCorrectedFromApprovedToProposed>();

            await _sut
                .Given(streetNameWasCorrectedFromApprovedToProposed)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasCorrectedFromApprovedToProposed.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasCorrectedFromApprovedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Proposed
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereCorrected_ThenStreetNameHomonymAdditionsWereCorrected()
        {
            var streetNameHomonymAdditionsWereCorrected = new StreetNameHomonymAdditionsWereCorrected(
                _fixture.Create<MunicipalityId>(),
                _fixture.Create<PersistentLocalId>(),
                [
                    new StreetNameHomonymAddition("DFG", Language.Dutch)
                ]);
            ((ISetProvenance)streetNameHomonymAdditionsWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new StreetNameListDocument(
                    streetNameHomonymAdditionsWereCorrected.PersistentLocalId,
                    _fixture.Create<Municipality>(),
                    [
                        new("Bergstraat", Infrastructure.Elastic.Language.nl),
                        new("Rue de la montagne", Infrastructure.Elastic.Language.fr)
                    ],
                    [
                        new("ABC", Infrastructure.Elastic.Language.nl),
                        new("XYZ", Infrastructure.Elastic.Language.fr)
                    ],
                    StreetNameStatus.Current,
                    _fixture.Create<Provenance>().Timestamp
                )]);

            await _sut
                .Given(streetNameHomonymAdditionsWereCorrected)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                            doc.StreetNamePersistentLocalId == streetNameHomonymAdditionsWereCorrected.PersistentLocalId
                            && doc.VersionTimestamp == streetNameHomonymAdditionsWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && DetermineExpectedNameForLanguage(doc.HomonymAdditions, Language.Dutch) == "DFG"
                            && DetermineExpectedNameForLanguage(doc.HomonymAdditions, Language.French) == "XYZ"
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameHomonymAdditionsWereRemoved_ThenStreetNameHomonymAdditionsWereRemoved()
        {
            var streetNameHomonymAdditionsWereRemoved = new StreetNameHomonymAdditionsWereRemoved(
                _fixture.Create<MunicipalityId>(),
                _fixture.Create<PersistentLocalId>(),
                [Language.Dutch]);
            ((ISetProvenance)streetNameHomonymAdditionsWereRemoved).SetProvenance(_fixture.Create<Provenance>());

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new StreetNameListDocument(
                    streetNameHomonymAdditionsWereRemoved.PersistentLocalId,
                    _fixture.Create<Municipality>(),
                    [
                        new("Bergstraat", Infrastructure.Elastic.Language.nl),
                        new("Rue de la montagne", Infrastructure.Elastic.Language.fr)
                    ],
                    [
                        new("ABC", Infrastructure.Elastic.Language.nl),
                        new("XYZ", Infrastructure.Elastic.Language.fr)
                    ],
                    StreetNameStatus.Current,
                    _fixture.Create<Provenance>().Timestamp
                )]);

            await _sut
                .Given(streetNameHomonymAdditionsWereRemoved)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                            doc.StreetNamePersistentLocalId == streetNameHomonymAdditionsWereRemoved.PersistentLocalId
                            && doc.VersionTimestamp == streetNameHomonymAdditionsWereRemoved.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && DetermineExpectedNameForLanguage(doc.HomonymAdditions, Language.Dutch) == null
                            && DetermineExpectedNameForLanguage(doc.HomonymAdditions, Language.French) == "XYZ"
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejected_ThenStreetNameStatusWasChangedToRejected()
        {
            var streetNameWasRejected = _fixture.Create<StreetNameWasRejected>();

            await _sut
                .Given(streetNameWasRejected)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasRejected.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasRejected.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Rejected
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenStreetNameStatusWasChangedToRejected()
        {
            var streetNameWasRejectedBecauseOfMunicipalityMerger = _fixture.Create<StreetNameWasRejectedBecauseOfMunicipalityMerger>();

            await _sut
                .Given(streetNameWasRejectedBecauseOfMunicipalityMerger)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasRejectedBecauseOfMunicipalityMerger.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasRejectedBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Rejected
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRejectedToProposed_ThenStreetNameStatusWasChangedBackToProposed()
        {
            var streetNameWasCorrectedFromRejectedToProposed = _fixture.Create<StreetNameWasCorrectedFromRejectedToProposed>();

            await _sut
                .Given(streetNameWasCorrectedFromRejectedToProposed)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasCorrectedFromRejectedToProposed.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasCorrectedFromRejectedToProposed.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Proposed
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredV2_ThenStreetNameStatusWasChangedToRetired()
        {
            var streetNameWasRetiredV2 = _fixture.Create<StreetNameWasRetiredV2>();

            await _sut
                .Given(streetNameWasRetiredV2)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasRetiredV2.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasRetiredV2.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Retired
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenStreetNameStatusWasChangedToRetired()
        {
            var streetNameWasRetiredBecauseOfMunicipalityMerger = _fixture.Create<StreetNameWasRetiredBecauseOfMunicipalityMerger>();

            await _sut
                .Given(streetNameWasRetiredBecauseOfMunicipalityMerger)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasRetiredBecauseOfMunicipalityMerger.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasRetiredBecauseOfMunicipalityMerger.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Retired
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRenamed_ThenStreetNameStatusWasChangedToRetired()
        {
            var streetNameWasRenamed = _fixture.Create<StreetNameWasRenamed>();

            await _sut
                .Given(streetNameWasRenamed)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasRenamed.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasRenamed.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Retired
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasCorrectedFromRetiredToCurrent_ThenStreetNameStatusWasChangedBackToCurrent()
        {
            var streetNameWasCorrectedFromRetiredToCurrent = _fixture.Create<StreetNameWasCorrectedFromRetiredToCurrent>();

            await _sut
                .Given(streetNameWasCorrectedFromRetiredToCurrent)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.PartialUpdateDocument(
                        streetNameWasCorrectedFromRetiredToCurrent.PersistentLocalId,
                        It.Is<StreetNameListPartialDocument>(doc =>
                            doc.VersionTimestamp == streetNameWasCorrectedFromRetiredToCurrent.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && doc.Status == StreetNameStatus.Current
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereCorrected_ThenStreetNameNamesWereCorrected()
        {
            var persistentLocalId = _fixture.Create<PersistentLocalId>();

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new StreetNameListDocument(
                    persistentLocalId,
                    _fixture.Create<Municipality>(),
                    [
                        new(_fixture.Create<string>(), Infrastructure.Elastic.Language.nl)
                    ],
                    [],
                    StreetNameStatus.Current,
                    _fixture.Create<Provenance>().Timestamp
                )]);

            var streetNameNamesWereCorrected = new StreetNameNamesWereCorrected(
                _fixture.Create<MunicipalityId>(),
                persistentLocalId,
                new Names(
                [
                    new StreetNameName("Kapelstraat", Language.Dutch),
                    new StreetNameName("Rue de la chapelle", Language.French),
                    new StreetNameName("Kapellenstraate", Language.German),
                    new StreetNameName("Chapel street", Language.English)
                ]));
            ((ISetProvenance)streetNameNamesWereCorrected).SetProvenance(_fixture.Create<Provenance>());

            await _sut
                .Given(streetNameNamesWereCorrected)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                            doc.StreetNamePersistentLocalId == streetNameNamesWereCorrected.PersistentLocalId
                            && doc.VersionTimestamp == streetNameNamesWereCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && DetermineExpectedNameForLanguage(doc.Names, Language.Dutch) == "Kapelstraat"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.French) == "Rue de la chapelle"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.German) == "Kapellenstraate"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.English) == "Chapel street"
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameNamesWereChanged_ThenStreetNameNamesWereChanged()
        {
            var persistentLocalId = _fixture.Create<PersistentLocalId>();

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new StreetNameListDocument(
                    persistentLocalId,
                    _fixture.Create<Municipality>(),
                    [
                        new(_fixture.Create<string>(), Infrastructure.Elastic.Language.nl)
                    ],
                    [],
                    StreetNameStatus.Current,
                    _fixture.Create<Provenance>().Timestamp
                )]);

            var streetNameNamesWereChanged = new StreetNameNamesWereChanged(
                _fixture.Create<MunicipalityId>(),
                persistentLocalId,
                new Names(
                [
                    new StreetNameName("Kapelstraat", Language.Dutch),
                    new StreetNameName("Rue de la chapelle", Language.French),
                    new StreetNameName("Kapellenstraate", Language.German),
                    new StreetNameName("Chapel street", Language.English)
                ]));
            ((ISetProvenance)streetNameNamesWereChanged).SetProvenance(_fixture.Create<Provenance>());

            await _sut
                .Given(streetNameNamesWereChanged)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.UpdateDocument(
                        It.Is<StreetNameListDocument>(doc =>
                            doc.StreetNamePersistentLocalId == streetNameNamesWereChanged.PersistentLocalId
                            && doc.VersionTimestamp == streetNameNamesWereChanged.Provenance.Timestamp.ToBelgianDateTimeOffset()
                            && DetermineExpectedNameForLanguage(doc.Names, Language.Dutch) == "Kapelstraat"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.French) == "Rue de la chapelle"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.German) == "Kapellenstraate"
                            && DetermineExpectedNameForLanguage(doc.Names, Language.English) == "Chapel street"
                        ),
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenMunicipalityNisCodeWasChanged_ThenMunicipalityNisCodeIsUpdatedAndLinkedStreetNamesHaveNewNisCode()
        {
            var municipalityNisCodeWasChanged = _fixture.Create<MunicipalityNisCodeWasChanged>();

            await CreateMunicipality(municipalityNisCodeWasChanged.NisCode);

            _elasticSearchClient
                .Setup(x => x.GetDocuments(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(municipalityNisCodeWasChanged.StreetNamePersistentLocalIds.Select(persistentLocalId => new StreetNameListDocument(
                    persistentLocalId,
                    _fixture.Create<Municipality>(),
                    [],
                    [],
                    StreetNameStatus.Current,
                    _fixture.Create<Provenance>().Timestamp
                )).ToList());

            await _sut
                .Given(municipalityNisCodeWasChanged)
                .Then(_ =>
                {
                    foreach (var persistentLocalId in municipalityNisCodeWasChanged.StreetNamePersistentLocalIds)
                    {
                        _elasticSearchClient.Verify(x => x.UpdateDocument(
                            It.Is<StreetNameListDocument>(doc =>
                                doc.StreetNamePersistentLocalId == persistentLocalId
                                && doc.VersionTimestamp == municipalityNisCodeWasChanged.Provenance.Timestamp.ToBelgianDateTimeOffset()
                                && doc.Municipality.NisCode == municipalityNisCodeWasChanged.NisCode
                            ),
                            It.IsAny<CancellationToken>()));
                    }

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenStreetNameWasRemovedV2_ThenStreetNameIsRemoved()
        {
            var streetNameWasRemovedV2 = _fixture.Create<StreetNameWasRemovedV2>();

            await _sut
                .Given(streetNameWasRemovedV2)
                .Then(_ =>
                {
                    _elasticSearchClient.Verify(x => x.DeleteDocument(
                        streetNameWasRemovedV2.PersistentLocalId,
                        It.IsAny<CancellationToken>()));

                    return Task.CompletedTask;
                });
        }

        private async Task CreateMunicipality(string nisCode)
        {
            _syndicationContext.MunicipalityLatestItems.Add(new()
            {
                MunicipalityId = Guid.NewGuid(),
                NisCode = nisCode
            });
            await _syndicationContext.SaveChangesAsync();
        }

        private static string? DetermineExpectedNameForLanguage(StreetNameRegistry.Infrastructure.Elastic.Name[] names, Language language)
            => names.Where(x => x.Language == ConvertToElasticLanguage(language)).Select(x => x.Spelling).SingleOrDefault();

        private static bool Equals(StreetNameRegistry.Infrastructure.Elastic.Name[] documentNames, IDictionary<Language, string> eventNames)
        {
            if (eventNames.Count != documentNames.Length)
            {
                return false;
            }

            foreach (var (language, name) in eventNames)
            {
                var matchingName = documentNames.SingleOrDefault(x => x.Language == ConvertToElasticLanguage(language));
                if (matchingName is null || matchingName.Spelling != name)
                {
                    return false;
                }
            }

            return true;
        }

        private static Infrastructure.Elastic.Language ConvertToElasticLanguage(Language language)
        {
            return language switch
            {
                Language.Dutch => Infrastructure.Elastic.Language.nl,
                Language.French => Infrastructure.Elastic.Language.fr,
                Language.German => Infrastructure.Elastic.Language.de,
                Language.English => Infrastructure.Elastic.Language.en,
                _ => throw new NotImplementedException($"Unknown language '{language}'")
            };
        }
    }
}
