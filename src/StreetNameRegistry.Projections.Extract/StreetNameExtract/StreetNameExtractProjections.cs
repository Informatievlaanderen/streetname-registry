namespace StreetNameRegistry.Projections.Extract.StreetNameExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using StreetName.Events;
    using System;
    using System.Globalization;
    using System.Text;
    using StreetName.Events.Crab;

    public class StreetNameExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string InUse = "InGebruik";
        private const string Proposed = "Voorgesteld";
        private const string Retired = "Gehistoreerd";
        private const string IdUri = "https://data.vlaanderen.be/id/straatnaam";
        private readonly Encoding _encoding;

        public StreetNameExtractProjections(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<StreetNameWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .StreetNameExtract
                    .AddAsync(new StreetNameExtractItem
                    {
                        StreetNameId = message.Message.StreetNameId,
                        DbaseRecord = new StreetNameDbaseRecord
                        {
                            gemeenteid = { Value = message.Message.NisCode },
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().DateTime }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<StreetNamePersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        var persistentLocalId = message.Message.PersistentLocalId;
                        streetName.StreetNamePersistentLocalId = persistentLocalId;
                        UpdateId(streetName, persistentLocalId);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasNamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStraatnm(streetName, message.Message.Language, message.Message.Name);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStraatnm(streetName, message.Message.Language, message.Message.Name);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStraatnm(streetName, message.Message.Language, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameNameWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStraatnm(streetName, message.Message.Language, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, InUse);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, InUse);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, Retired);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        context.StreetNameExtract.Remove(streetName);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, Retired);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, Proposed);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, Proposed);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateStatus(streetName, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasDefined>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateHomoniemtv(streetName, message.Message.Language, message.Message.HomonymAddition);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateHomoniemtv(streetName, message.Message.Language, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateHomoniemtv(streetName, message.Message.Language, message.Message.HomonymAddition);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionWasCorrectedToCleared>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        UpdateHomoniemtv(streetName, message.Message.Language, null);
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        streetName.Complete = true;
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNameBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameExtract(
                    message.Message.StreetNameId,
                    streetName =>
                    {
                        streetName.Complete = false;
                        UpdateVersie(streetName, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<StreetNamePrimaryLanguageWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNamePrimaryLanguageWasDefined>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasCorrectedToCleared>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameSecondaryLanguageWasDefined>>(async (context, message, ct) => DoNothing());

            When<Envelope<StreetNameWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<StreetNameStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private void UpdateHomoniemtv(StreetNameExtractItem streetName, Language? language, string homonymAddition)
            => UpdateRecord(streetName, record =>
            {
                switch (language)
                {
                    case Language.Dutch:
                        streetName.HomonymDutch = homonymAddition?.Substring(0, Math.Min(homonymAddition.Length, 5));
                        break;
                    case Language.French:
                        streetName.HomonymFrench = homonymAddition?.Substring(0, Math.Min(homonymAddition.Length, 5));
                        break;
                    case Language.German:
                        streetName.HomonymGerman = homonymAddition?.Substring(0, Math.Min(homonymAddition.Length, 5));
                        break;
                    case Language.English:
                        streetName.HomonymEnglish = homonymAddition?.Substring(0, Math.Min(homonymAddition.Length, 5));
                        break;
                    case null:
                        streetName.HomonymUnknown = homonymAddition?.Substring(0, Math.Min(homonymAddition.Length, 5));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }
            });

        private void UpdateStraatnm(StreetNameExtractItem streetName, Language? language, string name)
            => UpdateRecord(streetName, record =>
            {
                switch (language)
                {
                    case Language.Dutch:
                        streetName.NameDutch = name;
                        break;
                    case Language.French:
                        streetName.NameFrench = name;
                        break;
                    case Language.German:
                        streetName.NameGerman = name;
                        break;
                    case Language.English:
                        streetName.NameEnglish = name;
                        break;
                    case null:
                        streetName.NameUnknown = name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }
            });

        private void UpdateStatus(StreetNameExtractItem streetName, string status)
            => UpdateRecord(streetName, record => record.status.Value = status);

        private void UpdateId(StreetNameExtractItem streetName, int id)
            => UpdateRecord(streetName, record =>
            {
                record.id.Value = $"{IdUri}/{id}";
                record.straatnmid.Value = id;
            });

        private void UpdateVersie(StreetNameExtractItem streetName, Instant timestamp)
            => UpdateRecord(streetName, record => record.versieid.Value = timestamp.ToBelgianDateTimeOffset().DateTime);

        private void UpdateRecord(StreetNameExtractItem municipality, Action<StreetNameDbaseRecord> updateFunc)
        {
            var record = new StreetNameDbaseRecord();
            record.FromBytes(municipality.DbaseRecord, _encoding);

            updateFunc(record);

            municipality.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}
