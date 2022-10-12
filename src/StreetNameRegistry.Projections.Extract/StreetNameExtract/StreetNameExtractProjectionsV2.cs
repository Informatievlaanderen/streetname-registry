namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Options;
    using Municipality;
    using Municipality.Events;
    using NodaTime;

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

        public StreetNameExtractProjectionsV2(IOptions<ExtractConfig> extractConfig, Encoding encoding)
        {
            _extractConfig = extractConfig.Value ?? throw new ArgumentNullException(nameof(extractConfig));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, InUse);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, Proposed);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, Rejected);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, Proposed);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, Retired);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStatus(x, InUse);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(message.Message.PersistentLocalId, x =>
                {
                    UpdateStraatnm(x, message.Message.StreetNameNames);
                    UpdateVersie(x, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var streetNameExtractItemV2 = new StreetNameExtractItemV2
                {
                    StreetNamePersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    DbaseRecord = new StreetNameDbaseRecord
                    {
                        gemeenteid = { Value = message.Message.NisCode },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding)
                };
                UpdateId(streetNameExtractItemV2, message.Message.PersistentLocalId);
                UpdateStraatnm(streetNameExtractItemV2, new Names(message.Message.Names));
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
                    DbaseRecord = new StreetNameDbaseRecord
                    {
                        gemeenteid = { Value = message.Message.NisCode},
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

        private void UpdateStraatnm(StreetNameExtractItemV2 streetName, List<StreetNameName> streetNameNames)
            => UpdateRecord(streetName, record =>
            {
                foreach (var streetNameName in streetNameNames)
                {
                    switch (streetNameName.Language)
                    {
                        case Language.Dutch:
                            streetName.NameDutch = streetNameName.Name;
                            break;
                        case Language.French:
                            streetName.NameFrench = streetNameName.Name;
                            break;
                        case Language.German:
                            streetName.NameGerman = streetNameName.Name;
                            break;
                        case Language.English:
                            streetName.NameEnglish = streetNameName.Name;
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

        private void UpdateRecord(StreetNameExtractItemV2 municipality, Action<StreetNameDbaseRecord> updateFunc)
        {
            var record = new StreetNameDbaseRecord();
            record.FromBytes(municipality.DbaseRecord, _encoding);

            updateFunc(record);

            municipality.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
