namespace StreetNameRegistry.Projections.Feed.StreetNameFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    [ConnectedProjectionName("Feed endpoint straatnaam")]
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
                    message.Message.Names.Select(x =>
                        new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList());

                document.Document.Status = MapStatus(message.Message.Status);
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
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var document = new StreetNameDocument(
                    message.Message.PersistentLocalId,
                    message.Message.NisCode,
                    message.Message.Provenance.Timestamp,
                    message.Message.StreetNameNames.Select(x =>
                        new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList());
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
                    message.Message.StreetNameNames.Select(x =>
                        new GeografischeNaam(x.Value, MapLanguage(x.Key))).ToList());

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
            });
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
    }
}
