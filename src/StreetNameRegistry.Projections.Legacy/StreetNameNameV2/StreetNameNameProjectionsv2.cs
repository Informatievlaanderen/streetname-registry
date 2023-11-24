namespace StreetNameRegistry.Projections.Legacy.StreetNameNameV2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using Municipality.Events;
    using NodaTime;
    using StreetName.Events;

    [ConnectedProjectionName("API endpoint straatnamen ifv BOSA DT")]
    [ConnectedProjectionDescription("Projectie die de straatnamen data voor straatnamen ifv BOSA DT voorziet.")]
    public sealed class StreetNameNameProjectionsV2 : ConnectedProjection<LegacyContext>
    {
        public StreetNameNameProjectionsV2()
        {
            When<Envelope<StreetNameWasMigratedToMunicipality>>(async (context, message, ct) =>
            {
                var streetNameNameV2 = new StreetNameNameV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Removed = message.Message.IsRemoved,
                    Status = message.Message.Status,
                    IsFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode)
                };

                UpdateNameByLanguage(streetNameNameV2, message.Message.Names);

                await context
                    .StreetNameNamesV2
                    .AddAsync(streetNameNameV2, ct);
            });

            When<Envelope<StreetNameWasProposedV2>>(async (context, message, ct) =>
            {
                var streetNameNameV2 = new StreetNameNameV2
                {
                    PersistentLocalId = message.Message.PersistentLocalId,
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    IsFlemishRegion = RegionFilter.IsFlemishRegion(message.Message.NisCode),
                    Status = StreetNameStatus.Proposed,
                    Removed = false
                };

                UpdateNameByLanguage(streetNameNameV2, message.Message.StreetNameNames);

                await context
                    .StreetNameNamesV2
                    .AddAsync(streetNameNameV2, ct);
            });

            When<Envelope<StreetNameWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Rejected);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Proposed);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasRenamed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Retired);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateStatus(streetNameNameV2, StreetNameStatus.Current);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateNameByLanguage(streetNameNameV2, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateNameByLanguage(streetNameNameV2, message.Message.StreetNameNames);
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var streetNames = context
                    .StreetNameNamesV2
                    .Local
                    .Where(s => s.MunicipalityId == message.Message.MunicipalityId)
                    .Union(context.StreetNameNamesV2.Where(s => s.MunicipalityId == message.Message.MunicipalityId));

                foreach (var streetName in streetNames)
                {
                    streetName.NisCode = message.Message.NisCode;
                }

                await Task.Yield();
            });

            When<Envelope<StreetNameWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateStreetNameName(message.Message.PersistentLocalId, streetNameNameV2 =>
                {
                    streetNameNameV2.Removed = true;
                    UpdateVersionTimestamp(streetNameNameV2, message.Message.Provenance.Timestamp);
                }, ct);
            });
        }

        private static void UpdateNameByLanguage(StreetNameNameV2 entity, IDictionary<Language, string> streetNameNames)
        {
            foreach (var (language, streetNameName) in streetNameNames)
            {
                switch (language)
                {
                    case Language.Dutch:
                        entity.NameDutch = streetNameName;
                        entity.NameDutchSearch = streetNameName.SanitizeForBosaSearch();
                        break;

                    case Language.French:
                        entity.NameFrench = streetNameName;
                        entity.NameFrenchSearch = streetNameName.SanitizeForBosaSearch();
                        break;

                    case Language.German:
                        entity.NameGerman = streetNameName;
                        entity.NameGermanSearch = streetNameName.SanitizeForBosaSearch();
                        break;

                    case Language.English:
                        entity.NameEnglish = streetNameName;
                        entity.NameEnglishSearch = streetNameName.SanitizeForBosaSearch();
                        break;
                }
            }
        }

        private static void UpdateVersionTimestamp(StreetNameNameV2 streetNameNameV2, Instant versionTimestamp)
            => streetNameNameV2.VersionTimestamp = versionTimestamp;

        private static void UpdateStatus(StreetNameNameV2 streetNameNameV2, StreetNameStatus status)
            => streetNameNameV2.Status = status;
    }
}
