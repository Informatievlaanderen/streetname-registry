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
    using Consumer;
    using global::Elastic.Clients.Elasticsearch;
    using Legacy;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using StreetNameRegistry.Municipality;
    using StreetNameRegistry.Municipality.Events;
    using StreetNameRegistry.Projections.Elastic;
    using Language = StreetNameRegistry.Infrastructure.Elastic.Language;
    using Name = StreetNameRegistry.Infrastructure.Elastic.Name;

    [ConnectedProjectionName("API endpoint lijst adressen (elastic)")]
    [ConnectedProjectionDescription("Projectie die de data voor het adressenlijst endpoint in Elastic Search synchroniseert.")]
    public class StreetNameListProjections : ConnectedProjection<ElasticRunnerContext>
    {
        private readonly IDictionary<string, Municipality> _municipalities = new Dictionary<string, Municipality>();

        private readonly IStreetNameListElasticClient _searchElasticClient;
        private readonly IDbContextFactory<LegacyContext> _municipalityConsumerContextFactory;

        public StreetNameListProjections(
            IStreetNameListElasticClient searchElasticClient,
            IDbContextFactory<LegacyContext> municipalityConsumerContextFactory
            )
        {
            _searchElasticClient = searchElasticClient;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    [],
                    message.Message.Status,
                    //PrimaryLanguage = municipality.PrimaryLanguage, //TODO-pr is dit wel nodig?
                    RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.Names);
                UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    [],
                    StreetNameStatus.Proposed,
                    RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.StreetNameNames);
                UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var municipality = await GetMunicipality(message.Message.NisCode, ct);

                var document = new StreetNameListDocument(
                    message.Message.PersistentLocalId,
                    municipality,
                    [],
                    [],
                    [],
                    StreetNameStatus.Proposed,
                    RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    message.Message.Provenance.Timestamp
                );

                UpdateNameByLanguage(document, message.Message.StreetNameNames);

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Current
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Retired
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.PersistentLocalId,
                    new StreetNameListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = StreetNameStatus.Current
                    },
                    ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateNameByLanguage(document, message.Message.StreetNameNames);
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateNameByLanguage(document, message.Message.StreetNameNames);
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await UpdateDocuments([message.Message.PersistentLocalId], document =>
                {
                    UpdateHomonymAdditionByLanguage(document, new HomonymAdditions(message.Message.HomonymAdditions));
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
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
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<MunicipalityWasImported>>(async (context, message, ct) =>
            {
                throw new NotImplementedException();
                // var streetNameListMunicipality = new StreetNameListMunicipality
                // {
                //     MunicipalityId = message.Message.MunicipalityId,
                //     NisCode = message.Message.NisCode
                // };
                //
                // await context
                //     .StreetNameListMunicipality
                //     .AddAsync(streetNameListMunicipality, ct);
            });

            When<Envelope<MunicipalityOfficialLanguageWasAdded>>(async (context, message, ct) =>
            {
                throw new NotImplementedException();
                // var municipality =
                //     await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId, cancellationToken: ct);
                //
                // if (municipality.PrimaryLanguage == null)
                // {
                //     municipality.PrimaryLanguage = message.Message.Language;
                // }
                // else if (municipality.SecondaryLanguage == null)
                // {
                //     municipality.SecondaryLanguage = message.Message.Language;
                // }
                // else
                // {
                //     throw new InvalidOperationException(
                //         $"Cannot add an official language while primary and secondary are assigned for municipality {municipality.MunicipalityId}");
                // }
            });

            When<Envelope<MunicipalityOfficialLanguageWasRemoved>>(async (context, message, ct) =>
            {
                throw new NotImplementedException();
                // var municipality =
                //     await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId, cancellationToken: ct);
                //
                // if (municipality.SecondaryLanguage == message.Message.Language)
                // {
                //     municipality.SecondaryLanguage = null;
                // }
                // else if (municipality.PrimaryLanguage == message.Message.Language)
                // {
                //     municipality.PrimaryLanguage = null;
                //
                //     // if official is removed for primary, but still has secondary, then move secondary to primary
                //     if (municipality.SecondaryLanguage != null)
                //     {
                //         municipality.PrimaryLanguage = municipality.SecondaryLanguage;
                //     }
                // }
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                throw new NotImplementedException();
                // var municipality =
                //     await context.StreetNameListMunicipality.FindAsync(message.Message.MunicipalityId,
                //         cancellationToken: ct);
                //
                // municipality.NisCode = message.Message.NisCode;
                //
                // var streetNames = context
                //     .StreetNameListV2
                //     .Local
                //     .Where(s => s.MunicipalityId == message.Message.MunicipalityId)
                //     .Union(context.StreetNameListV2.Where(s => s.MunicipalityId == message.Message.MunicipalityId));
                //
                // var streetNameIsInFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode);
                // foreach (var streetName in streetNames)
                // {
                //     streetName.NisCode = message.Message.NisCode;
                //     streetName.IsInFlemishRegion = streetNameIsInFlemishRegion;
                // }
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await searchElasticClient.DeleteDocument(
                    message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<MunicipalityBecameCurrent>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasAdded>>(DoNothing);
            When<Envelope<MunicipalityFacilityLanguageWasRemoved>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToCurrent>>(DoNothing);
            When<Envelope<MunicipalityWasCorrectedToRetired>>(DoNothing);
            When<Envelope<MunicipalityWasMerged>>(DoNothing);
            When<Envelope<MunicipalityWasNamed>>(DoNothing);
            When<Envelope<MunicipalityWasRetired>>(DoNothing);
            When<Envelope<MunicipalityWasRemoved>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);
        }

        private static void UpdateNameByLanguage(StreetNameListDocument entity, IDictionary<StreetNameRegistry.Municipality.Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case StreetNameRegistry.Municipality.Language.Dutch:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.nl, streetNameName);
                        entity.SearchNames = UpdateLanguageValue(entity.SearchNames, Language.nl, streetNameName.RemoveDiacritics()!);
                        break;

                    case StreetNameRegistry.Municipality.Language.French:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.fr, streetNameName);
                        entity.SearchNames = UpdateLanguageValue(entity.SearchNames, Language.fr, streetNameName.RemoveDiacritics()!);
                        break;

                    case StreetNameRegistry.Municipality.Language.German:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.de, streetNameName);
                        entity.SearchNames = UpdateLanguageValue(entity.SearchNames, Language.de, streetNameName.RemoveDiacritics()!);
                        break;

                    case StreetNameRegistry.Municipality.Language.English:
                        entity.Names = UpdateLanguageValue(entity.Names, Language.en, streetNameName);
                        entity.SearchNames = UpdateLanguageValue(entity.SearchNames, Language.en, streetNameName.RemoveDiacritics()!);
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
                        throw new ArgumentOutOfRangeException(nameof(homonymAddition.Language), homonymAddition.Language, null);
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
        //
        // private async Task UpdateStreetNameAndFullAddress(
        //     int streetNamePersistentLocalId,
        //     ICollection<int> streetNamePersistentLocalIds,
        //     Instant versionTimestamp,
        //     CancellationToken ct)
        // {
        //     var streetNameLatestItem = await RefreshStreetNames(streetNamePersistentLocalId, ct);
        //
        //     var documents = await _searchElasticClient.GetDocuments(streetNamePersistentLocalIds, ct);
        //
        //     foreach (var streetNamePersistentLocalId in streetNamePersistentLocalIds)
        //     {
        //         var document = documents.SingleOrDefault(x => x.StreetNamePersistentLocalId == streetNamePersistentLocalId);
        //         if (document is null)
        //         {
        //             throw new InvalidOperationException($"No document received for {streetNamePersistentLocalId}");
        //         }
        //
        //         var desiredVersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset() > document.VersionTimestamp
        //             ? versionTimestamp.ToBelgianDateTimeOffset()
        //             : document.VersionTimestamp;
        //         var streetName = StreetName.FromStreetNameLatestItem(streetNameLatestItem);
        //
        //         document.VersionTimestamp = desiredVersionTimestamp;
        //         document.StreetName = streetName;
        //
        //         await _searchElasticClient.UpdateDocument(document, ct);
        //     }
        // }

        private async Task<Municipality> GetMunicipality(string nisCode, CancellationToken ct)
        {
            if (_municipalities.TryGetValue(nisCode, out var value))
                return value;

            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync(ct);
            var municipalityLatestItem = await context.StreetNameListMunicipality
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipalityLatestItem == null)
                throw new InvalidOperationException($"Municipality with NisCode {nisCode} not found");

            _municipalities.Add(nisCode, Municipality.FromMunicipalityLatestItem(municipalityLatestItem));

            return _municipalities[nisCode];
        }

        private static Task DoNothing<T>(ElasticRunnerContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
