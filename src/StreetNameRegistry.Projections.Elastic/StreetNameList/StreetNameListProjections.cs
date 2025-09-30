namespace StreetNameRegistry.Projections.Elastic.StreetNameList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using StreetNameRegistry.Municipality;
    using StreetNameRegistry.Municipality.Events;
    using Syndication;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;
    using Name = StreetNameRegistry.Infrastructure.Elastic.Name;

    [ConnectedProjectionName("API endpoint lijst straatnamen (elastic)")]
    [ConnectedProjectionDescription("Projectie die de data voor de straatnamenlijst endpoint in Elastic Search synchroniseert.")]
    public class StreetNameListProjections : ConnectedProjection<ElasticRunnerContext>
    {
        private readonly IDictionary<string, Municipality> _municipalities = new Dictionary<string, Municipality>();

        private readonly IStreetNameListElasticClient _searchElasticClient;
        private readonly IDbContextFactory<SyndicationContext> _syndicationContextFactory;

        public StreetNameListProjections(
            IStreetNameListElasticClient searchElasticClient,
            IDbContextFactory<SyndicationContext> syndicationContextFactory
        )
        {
            _searchElasticClient = searchElasticClient;
            _syndicationContextFactory = syndicationContextFactory;

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    message.Message.Status,
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.Names);
                UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    StreetNameStatus.Proposed,
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.StreetNameNames);
                UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    StreetNameStatus.Proposed,
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.StreetNameNames);

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Current
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Current
                    },
                    ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateNameByLanguage(document, message.Message.StreetNameNames);
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateNameByLanguage(document, message.Message.StreetNameNames);
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case StreetNameRegistry.Municipality.Language.Dutch:
                                document.HomonymAdditions = document.HomonymAdditions.Where(x => x.Language != Language.nl).ToArray();
                                break;
                            case StreetNameRegistry.Municipality.Language.French:
                                document.HomonymAdditions = document.HomonymAdditions.Where(x => x.Language != Language.fr).ToArray();
                                break;
                            case StreetNameRegistry.Municipality.Language.German:
                                document.HomonymAdditions = document.HomonymAdditions.Where(x => x.Language != Language.de).ToArray();
                                break;
                            case StreetNameRegistry.Municipality.Language.English:
                                document.HomonymAdditions = document.HomonymAdditions.Where(x => x.Language != Language.en).ToArray();
                                break;
                            default:
                                throw new NotImplementedException($"Language {language} not implemented");
                        }
                    }
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (_, message, ct) =>
            {
                await searchElasticClient.DeleteDocument(
                    message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (_, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                await UpdateDocuments(message.Message.StreetNamePersistentLocalIds, document =>
                {
                    document.Municipality = municipality;
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<MunicipalityWasImported>>(DoNothing);
            When<Envelope<MunicipalityBecameCurrent>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Envelope<MunicipalityWasMerged>>(DoNothing);
            When<Envelope<MunicipalityWasNamed>>(DoNothing);
            When<Envelope<MunicipalityWasRetired>>(DoNothing);
            When<Envelope<MunicipalityWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityOfficialLanguageWasAdded>>(DoNothing); // Event will happen before StreetNames are created
            When<Envelope<MunicipalityOfficialLanguageWasRemoved>>(DoNothing);
        }

        private static void UpdateNameByLanguage(StreetNameListDocument entity, IDictionary<StreetNameRegistry.Municipality.Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case StreetNameRegistry.Municipality.Language.Dutch:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.nl, streetNameName);
                        break;

                    case StreetNameRegistry.Municipality.Language.French:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.fr, streetNameName);
                        break;

                    case StreetNameRegistry.Municipality.Language.German:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.de, streetNameName);
                        break;

                    case StreetNameRegistry.Municipality.Language.English:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.en, streetNameName);
                        break;
                }
            }
        }

        private static void UpdateHomonymAdditionByLanguage(StreetNameListDocument entity, List<StreetNameHomonymAddition> homonymAdditions)
        {
            foreach (var homonymAddition in homonymAdditions)
            {
                switch (homonymAddition.Language)
                {
                    case StreetNameRegistry.Municipality.Language.Dutch:
                        entity.HomonymAdditions = UpdateLanguageValue(entity.HomonymAdditions, Language.nl, homonymAddition.HomonymAddition);
                        break;

                    case StreetNameRegistry.Municipality.Language.French:
                        entity.HomonymAdditions = UpdateLanguageValue(entity.HomonymAdditions, Language.fr, homonymAddition.HomonymAddition);
                        break;

                    case StreetNameRegistry.Municipality.Language.German:
                        entity.HomonymAdditions = UpdateLanguageValue(entity.HomonymAdditions, Language.de, homonymAddition.HomonymAddition);
                        break;

                    case StreetNameRegistry.Municipality.Language.English:
                        entity.HomonymAdditions = UpdateLanguageValue(entity.HomonymAdditions, Language.en, homonymAddition.HomonymAddition);
                        break;
                    default:
                        throw new NotImplementedException($"Language {homonymAddition.Language} not implemented");
                }
            }
        }

        private static Name[] UpdateLanguageValue(Name[] names, Language language, string value)
        {
            return names
                .Where(x => x.Language != language)
                .Concat([new Name(value, language)])
                .ToArray();
        }

        private async Task UpdateDocuments(
            ICollection<int> streetNamePersistentLocalIds,
            Action<StreetNameListDocument> update,
            Instant versionTimestamp,
            CancellationToken ct)
        {
            var documents = await _searchElasticClient.GetDocuments(streetNamePersistentLocalIds, ct);

            foreach (var streetNamePersistentLocalId in streetNamePersistentLocalIds)
            {
                var document = documents.SingleOrDefault(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
                if (document is null)
                {
                    throw new InvalidOperationException($"No document received for {streetNamePersistentLocalId}");
                }

                update(document);

                document.VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();

                await _searchElasticClient.UpdateDocument(document, ct);
            }
        }

        private async Task<Municipality> GetMunicipality(string nisCode, CancellationToken ct)
        {
            if (_municipalities.TryGetValue(nisCode, out var value))
                return value;

            await using var context = await _syndicationContextFactory.CreateDbContextAsync(ct);
            var municipalityLatestItem = await context.MunicipalityLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipalityLatestItem == null)
                throw new InvalidOperationException($"Municipality with NisCode {nisCode} not found");

            _municipalities.Add(nisCode, Municipality.FromMunicipalityLatestItem(municipalityLatestItem));

            return _municipalities[nisCode];
        }

        private static Task DoNothing<T>(ElasticRunnerContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}
