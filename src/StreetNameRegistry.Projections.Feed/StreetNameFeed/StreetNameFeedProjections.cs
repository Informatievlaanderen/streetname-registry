namespace StreetNameRegistry.Projections.Feed.StreetNameFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using Municipality.Events;

    [ConnectedProjectionName("Feed endpoint straatnamen (cloudevents)")]
    [ConnectedProjectionDescription("Projectie die de straatnaam data voor de straatnaam cloudevent feed voorziet.")]
    public class StreetNameFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        public StreetNameFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var document = new StreetNameDocument(
                    message.Message.PersistentLocalId,
                    message.Message.NisCode,
                    message.Message.Provenance.Timestamp,
                    MapNames(message.Message.Names));

                document.Document.Status = MapStatus(message.Message.Status);
                document.Document.HomonymAdditions = message.Message.HomonymAdditions.Select(x =>
                    new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList();
                await context.StreetNameDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> baseRegistriesCloudEventAttributes = [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.MunicipalityId, null, OsloNamespaces.Gemeente.ToPuri(document.Document.NisCode)),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StreetNameNames, null, document.Document.Names)
                ];

                if (document.Document.HomonymAdditions.Any())
                    baseRegistriesCloudEventAttributes.Add(new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.HomonymAdditions, null, document.Document.HomonymAdditions));

                await AddCloudEvent(message, document, context, baseRegistriesCloudEventAttributes, StreetNameEventTypes.CreateV1);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var document = new StreetNameDocument(
                    message.Message.PersistentLocalId,
                    message.Message.NisCode,
                    message.Message.Provenance.Timestamp,
                    MapNames(message.Message.StreetNameNames));
                await context.StreetNameDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.MunicipalityId, null, OsloNamespaces.Gemeente.ToPuri(document.Document.NisCode)),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, null, StraatnaamStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StreetNameNames, null, document.Document.Names)
                ], StreetNameEventTypes.CreateV1);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.InGebruik;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.InGebruik)
                ]);
            });

            When<Envelope<StreetNameWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = new StreetNameDocument(
                    message.Message.PersistentLocalId,
                    message.Message.NisCode,
                    message.Message.Provenance.Timestamp,
                    MapNames(message.Message.StreetNameNames));

                document.Document.Status = StraatnaamStatus.Voorgesteld;
                document.Document.HomonymAdditions = message.Message.HomonymAdditions.Select(x =>
                    new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList();
                await context.StreetNameDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> baseRegistriesCloudEventAttributes = [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.MunicipalityId, null, OsloNamespaces.Gemeente.ToPuri(document.Document.NisCode)),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, null, StraatnaamStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StreetNameNames, null, document.Document.Names)
                ];

                if (document.Document.HomonymAdditions.Any())
                    baseRegistriesCloudEventAttributes.Add(new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.HomonymAdditions, null, document.Document.HomonymAdditions));

                await AddCloudEvent(message, document, context, baseRegistriesCloudEventAttributes, StreetNameEventTypes.CreateV1);

                // var page = await context.CalculatePage();
                // var streetNameFeedItem = new StreetNameFeedItem(
                //     position: message.Position,
                //     page: page,
                //     persistentLocalId: document.PersistentLocalId)
                // {
                //     Application = message.Message.Provenance.Application,
                //     Modification = message.Message.Provenance.Modification,
                //     Operator = message.Message.Provenance.Operator,
                //     Organisation = message.Message.Provenance.Organisation,
                //     Reason = message.Message.Provenance.Reason
                // };
                // await context.StreetNameFeed.AddAsync(streetNameFeedItem, ct);
                // var nisCodes = context
                //     .StreetNameDocuments
                //     .Local
                //     .Where(x => message.Message.MergedStreetNamePersistentLocalIds.Contains(x.PersistentLocalId))
                //     .Select(x => x.Document.NisCode)
                //     .Union(context
                //         .StreetNameDocuments
                //         .Where(x => message.Message.MergedStreetNamePersistentLocalIds.Contains(x.PersistentLocalId))
                //         .Select(x => x.Document.NisCode))
                //     .ToList();
                //
                // nisCodes.Add(document.Document.NisCode);
                // nisCodes = nisCodes.Distinct().ToList();
                //
                // var cloudEvent = _changeFeedService.CreateCloudEvent(
                //     streetNameFeedItem.Id,
                //     message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                //     StreetNameEventTypes.TransformV1,
                //     new StreetNameCloudTransformEvent
                //     {
                //         NisCodes =  nisCodes,
                //         To = [OsloNamespaces.StraatNaam.ToPuri(document.PersistentLocalId.ToString())],
                //         From = message.Message.MergedStreetNamePersistentLocalIds.Select(id => OsloNamespaces.StraatNaam.ToPuri(id.ToString())).ToList()
                //     },
                //     _changeFeedService.DataSchemaUriTransform,
                //     message.EventName,
                //     message.Metadata["CommandId"].ToString()!);

                //streetNameFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
                //await CheckToUpdateCache(page, context);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Voorgesteld;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Voorgesteld)
                ]);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Afgekeurd)
                ]);
            });

            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var page = await context.CalculatePage();
                var streetNameFeedItem = new StreetNameFeedItem(
                    position: message.Position,
                    page: page,
                    persistentLocalId: document.PersistentLocalId)
                {
                    Application = message.Message.Provenance.Application,
                    Modification = message.Message.Provenance.Modification,
                    Operator = message.Message.Provenance.Operator,
                    Organisation = message.Message.Provenance.Organisation,
                    Reason = message.Message.Provenance.Reason
                };
                await context.StreetNameFeed.AddAsync(streetNameFeedItem, ct);
                var nisCodes = context
                    .StreetNameDocuments
                    .Local
                    .Where(x => message.Message.NewPersistentLocalIds.Contains(x.PersistentLocalId))
                    .Select(x => x.Document.NisCode)
                    .Union(context
                        .StreetNameDocuments
                        .Where(x => message.Message.NewPersistentLocalIds.Contains(x.PersistentLocalId))
                        .Select(x => x.Document.NisCode))
                    .ToList();

                nisCodes.Add(document.Document.NisCode);
                nisCodes = nisCodes.Distinct().ToList();

                var cloudEvent = _changeFeedService.CreateCloudEvent(
                    streetNameFeedItem.Id,
                    message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                    StreetNameEventTypes.TransformV1,
                    new StreetNameCloudTransformEvent
                    {
                        NisCodes =  nisCodes,
                        From = [OsloNamespaces.StraatNaam.ToPuri(document.PersistentLocalId.ToString())],
                        To = message.Message.NewPersistentLocalIds.Select(id => OsloNamespaces.StraatNaam.ToPuri(id.ToString())).ToList()
                    },
                    _changeFeedService.DataSchemaUriTransform,
                    message.EventName,
                    message.Metadata["CommandId"].ToString()!);

                streetNameFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
                await CheckToUpdateCache(page, context);

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Afgekeurd)
                ]);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Voorgesteld;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Voorgesteld)
                ]);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var page = await context.CalculatePage();
                var streetNameFeedItem = new StreetNameFeedItem(
                    position: message.Position,
                    page: page,
                    persistentLocalId: document.PersistentLocalId)
                {
                    Application = message.Message.Provenance.Application,
                    Modification = message.Message.Provenance.Modification,
                    Operator = message.Message.Provenance.Operator,
                    Organisation = message.Message.Provenance.Organisation,
                    Reason = message.Message.Provenance.Reason
                };
                await context.StreetNameFeed.AddAsync(streetNameFeedItem, ct);
                var nisCodes = context
                    .StreetNameDocuments
                    .Local
                    .Where(x => message.Message.NewPersistentLocalIds.Contains(x.PersistentLocalId))
                    .Select(x => x.Document.NisCode)
                    .Union(context
                        .StreetNameDocuments
                        .Where(x => message.Message.NewPersistentLocalIds.Contains(x.PersistentLocalId))
                        .Select(x => x.Document.NisCode))
                    .ToList();

                nisCodes.Add(document.Document.NisCode);
                nisCodes = nisCodes.Distinct().ToList();

                var cloudEvent = _changeFeedService.CreateCloudEvent(
                    streetNameFeedItem.Id,
                    message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                    StreetNameEventTypes.TransformV1,
                    new StreetNameCloudTransformEvent
                    {
                        NisCodes =  nisCodes,
                        From = [OsloNamespaces.StraatNaam.ToPuri(document.PersistentLocalId.ToString())],
                        To = message.Message.NewPersistentLocalIds.Select(id => OsloNamespaces.StraatNaam.ToPuri(id.ToString())).ToList()
                    },
                    _changeFeedService.DataSchemaUriTransform,
                    message.EventName,
                    message.Metadata["CommandId"].ToString()!);

                streetNameFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
                await CheckToUpdateCache(page, context);

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var page = await context.CalculatePage();
                var streetNameFeedItem = new StreetNameFeedItem(
                    position: message.Position,
                    page: page,
                    persistentLocalId: document.PersistentLocalId)
                {
                    Application = message.Message.Provenance.Application,
                    Modification = message.Message.Provenance.Modification,
                    Operator = message.Message.Provenance.Operator,
                    Organisation = message.Message.Provenance.Organisation,
                    Reason = message.Message.Provenance.Reason
                };
                await context.StreetNameFeed.AddAsync(streetNameFeedItem, ct);

                var cloudEvent = _changeFeedService.CreateCloudEvent(
                    streetNameFeedItem.Id,
                    message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                    StreetNameEventTypes.TransformV1,
                    new StreetNameCloudTransformEvent
                    {
                        NisCodes =  [document.Document.NisCode],
                        From = [OsloNamespaces.StraatNaam.ToPuri(document.PersistentLocalId.ToString())],
                        To = [OsloNamespaces.StraatNaam.ToPuri(message.Message.DestinationPersistentLocalId.ToString())]
                    },
                    _changeFeedService.DataSchemaUriTransform,
                    message.EventName,
                    message.Metadata["CommandId"].ToString()!);

                streetNameFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
                await CheckToUpdateCache(page, context);

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldStatus = document.Document.Status;
                document.Document.Status = StraatnaamStatus.InGebruik;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StatusName, oldStatus, StraatnaamStatus.InGebruik)
                ]);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldNames = document.Document.Names;
                document.Document.Names = MapNames(message.Message.StreetNameNames);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StreetNameNames, oldNames, document.Document.Names)
                ]);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldNames = document.Document.Names;
                document.Document.Names = MapNames(message.Message.StreetNameNames);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.StreetNameNames, oldNames, document.Document.Names)
                ]);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldHomonyms = document.Document.HomonymAdditions;
                document.Document.HomonymAdditions = MapNames(message.Message.HomonymAdditions);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.HomonymAdditions, oldHomonyms, document.Document.HomonymAdditions)
                ]);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                var oldHomonyms = document.Document.HomonymAdditions.ToList();
                foreach (var language in message.Message.Languages)
                    document.Document.HomonymAdditions.Remove(
                        document.Document.HomonymAdditions.Single(x => x.Taal == MapLanguage(language)));

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(StreetNameAttributeNames.HomonymAdditions, oldHomonyms, document.Document.HomonymAdditions)
                ]);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                var document = await context.StreetNameDocuments.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);
                if (document is null)
                    throw new InvalidOperationException($"Could not find document for streetname {message.Message.PersistentLocalId}");

                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], StreetNameEventTypes.DeleteV1);
            });

            // What should we do here? This event isn't supposed to happen in the first place.
            When<Envelope<MunicipalityNisCodeWasChanged>>((context, message, ct) => throw new NotImplementedException());

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

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            StreetNameDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType = StreetNameEventTypes.UpdateV1)
            where T : IHasProvenance, IMessage
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();
            var streetNameFeedItem = new StreetNameFeedItem(
                position: message.Position,
                page: page,
                persistentLocalId: document.PersistentLocalId)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.StreetNameFeed.AddAsync(streetNameFeedItem);

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                streetNameFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                [document.Document.NisCode],
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            streetNameFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p => await context.StreetNameFeed.CountAsync(x => x.Page == p));
        }

        private static List<GeografischeNaam> MapNames(IDictionary<Language, string> streetNameNames)
            => streetNameNames.Select(x => new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList();

        private static Taal MapLanguage(Language language)
        {
            switch (language)
            {
                default:
                case Language.Dutch:
                    return Taal.NL;

                case Language.French:
                    return Taal.FR;

                case Language.German:
                    return Taal.DE;

                case Language.English:
                    return Taal.EN;
            }
        }

        private static StraatnaamStatus MapStatus(StreetNameStatus status)
        {
            return status switch
            {
                StreetNameStatus.Proposed => StraatnaamStatus.Voorgesteld,
                StreetNameStatus.Current => StraatnaamStatus.InGebruik,
                StreetNameStatus.Retired => StraatnaamStatus.Gehistoreerd,
                StreetNameStatus.Rejected => StraatnaamStatus.Afgekeurd,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}
