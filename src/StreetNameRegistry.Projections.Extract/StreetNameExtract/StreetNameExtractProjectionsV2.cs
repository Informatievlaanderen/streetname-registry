namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName.Events;

    [ConnectedProjectionName("Extract straatnamen")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor het straatnamen extract voorziet.")]
    public sealed class StreetNameExtractProjectionsV2 : ConnectedProjection<ExtractContext>
    {
        private const string InUse = "InGebruik";
        private const string Rejected = "Afgekeurd";
        private const string Proposed = "Voorgesteld";
        private const string Retired = "Gehistoreerd";
        private readonly Encoding _encoding;
        private readonly ExtractConfig _extractConfig;

        public StreetNameExtractProjectionsV2(
            IReadonlyStreamStore streamStore,
            EventDeserializer eventDeserializer,
            IOptions<ExtractConfig> extractConfig,
            Encoding encoding)
        {
            _extractConfig = extractConfig.Value ?? throw new ArgumentNullException(nameof(extractConfig));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

             When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                    return;

                var firstEventJsonData = await streamStore
                    .ReadStreamForwards(message.Message.StreetNameId.ToString("D"), StreamVersion.Start, 1, cancellationToken: ct)
                    .GetAwaiter()
                    .GetResult()
                    .Messages
                    .First()
                    .GetJsonData(ct);

                var firstEvent = (IHasProvenance) eventDeserializer.DeserializeObject(firstEventJsonData, typeof(StreetNameWasRegistered));
                var creationDateAsString = firstEvent.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset();

                var streetNameExtractItemV2 = new StreetNameExtractItemV2
                {
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    DbaseRecord = new StreetNameDbaseRecordV2
                    {
                        gemeenteid = { Value = message.Message.NisCode },
                        creatieid = { Value = creationDateAsString },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };
                UpdateId(streetNameExtractItemV2, message.Message.PersistentLocalId);
                UpdateStraatnm(streetNameExtractItemV2, message.Message.Names);
                UpdateHomoniemtv(streetNameExtractItemV2, new HomonymAdditions(message.Message.HomonymAdditions));

                var status = message.Message.Status switch
                {
                    StreetNameStatus.Current => InUse,
                    StreetNameStatus.Proposed => Proposed,
                    StreetNameStatus.Retired => Retired,
                    _ => throw new ArgumentOutOfRangeException(nameof(message.Message.Status))
                };

                UpdateStatus(streetNameExtractItemV2, status);

                await context
                    .StreetNameExtractV2
                    .AddAsync(streetNameExtractItemV2, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var streetNameExtractItemV2 = new StreetNameExtractItemV2
                {
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    DbaseRecord = new StreetNameDbaseRecordV2
                    {
                        gemeenteid = { Value = message.Message.NisCode},
                        creatieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };
                UpdateId(streetNameExtractItemV2, message.Message.PersistentLocalId);
                UpdateStraatnm(streetNameExtractItemV2, message.Message.StreetNameNames);
                UpdateStatus(streetNameExtractItemV2, Proposed);
                await context
                    .StreetNameExtractV2
                    .AddAsync(streetNameExtractItemV2, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, InUse);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, Proposed);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, Rejected);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, Proposed);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, Retired);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, Retired);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                    {
                        UpdateStatus(x, Retired);
                        UpdateVersie(x, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStatus(x, InUse);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStraatnm(x, message.Message.StreetNameNames);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

             When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateStraatnm(x, message.Message.StreetNameNames);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    UpdateHomoniemtv(x, new HomonymAdditions(message.Message.HomonymAdditions));
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    x =>
                {
                    foreach (var language in message.Message.Languages)
                    {
                        switch (language)
                        {
                            case Language.Dutch: x.HomonymDutch = null;
                                break;
                            case Language.French: x.HomonymFrench = null;
                                break;
                            case Language.German: x.HomonymGerman = null;
                                break;
                            case Language.English: x.HomonymEnglish = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.MunicipalityId,
                    message.Message.PersistentLocalId,
                    streetName =>
                {
                    context.StreetNameExtractV2.Remove(streetName);
                }, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNames = context
                    .StreetNameExtractV2
                    .Local
                    .Where(s => s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNameExtractV2.Where(s => s.MunicipalityId == message.Message.MunicipalityId));

                foreach (var streetName in streetNames)
                {
                    UpdateRecord(streetName, i => i.gemeenteid.Value = message.Message.NisCode);
                }
            });
        }

        private void UpdateHomoniemtv(StreetNameExtractItemV2 streetName, List<StreetNameHomonymAddition> homonymAdditions)
            => UpdateRecord(streetName, record =>
            {
                foreach (var streetNameHomonymAddition in homonymAdditions)
                {
                    switch (streetNameHomonymAddition.Language)
                    {
                        case Language.Dutch:
                            streetName.HomonymDutch =
                                streetNameHomonymAddition.HomonymAddition.Substring(0, Math.Min(streetNameHomonymAddition.HomonymAddition.Length, 5));
                            break;
                        case Language.French:
                            streetName.HomonymFrench =
                                streetNameHomonymAddition.HomonymAddition.Substring(0, Math.Min(streetNameHomonymAddition.HomonymAddition.Length, 5));
                            break;
                        case Language.German:
                            streetName.HomonymGerman =
                                streetNameHomonymAddition.HomonymAddition.Substring(0, Math.Min(streetNameHomonymAddition.HomonymAddition.Length, 5));
                            break;
                        case Language.English:
                            streetName.HomonymEnglish =
                                streetNameHomonymAddition.HomonymAddition.Substring(0, Math.Min(streetNameHomonymAddition.HomonymAddition.Length, 5));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(streetNameHomonymAddition.Language), streetNameHomonymAddition.Language, null);
                    }
                }
            });

        private void UpdateStraatnm(StreetNameExtractItemV2 streetName, IDictionary<Language, string> streetNameNames)
            => UpdateRecord(streetName, record =>
            {
                foreach (var (language, streetNameName) in streetNameNames)
                {
                    switch (language)
                    {
                        case Language.Dutch:
                            streetName.NameDutch = streetNameName;
                            break;
                        case Language.French:
                            streetName.NameFrench = streetNameName;
                            break;
                        case Language.German:
                            streetName.NameGerman = streetNameName;
                            break;
                        case Language.English:
                            streetName.NameEnglish = streetNameName;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(streetName), streetName, null);
                    }
                }
            });

        private void UpdateId(StreetNameExtractItemV2 streetName, int id)
            => UpdateRecord(streetName, record =>
            {
                record.id.Value = $"{_extractConfig.DataVlaanderenNamespace}/{id}";
                record.straatnmid.Value = id;
            });

        private void UpdateStatus(StreetNameExtractItemV2 streetName, string status)
            => UpdateRecord(streetName, record => record.status.Value = status);

        private void UpdateVersie(StreetNameExtractItemV2 streetName, Instant timestamp)
            => UpdateRecord(streetName, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(StreetNameExtractItemV2 municipality, Action<StreetNameDbaseRecordV2> updateFunc)
        {
            var record = new StreetNameDbaseRecordV2();
            record.FromBytes(municipality.DbaseRecord, _encoding);

            updateFunc(record);

            municipality.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
