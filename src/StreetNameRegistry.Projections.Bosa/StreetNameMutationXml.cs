namespace StreetNameRegistry.Projections.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Municipality;
    using NodaTime;

    /// <summary>
    /// A reference to a version of a street name, as predecessor or successor.
    /// </summary>
    public sealed record StreetNameVersionLink(int PersistentLocalId, DateTimeOffset VersionId);

    /// <summary>
    /// Builds the addStreetNameVersion / updateStreetNameVersion elements of Mutations.xsd,
    /// with the street name as StreetNameType of BeStCommon.xsd.
    /// </summary>
    public static class StreetNameMutationXml
    {
        public static readonly XNamespace Mutations = "http://fsb.belgium.be/mappingservices/Mutations/v1_00";
        public static readonly XNamespace Common = "http://fsb.belgium.be/data/common";

        public const string MunicipalityNamespace = "https://data.vlaanderen.be/id/gemeente/";

        public static XElement BuildAdd(StreetNameVersionPart version, IEnumerable<StreetNameVersionLink> predecessors)
            => new XElement(Mutations + "addStreetNameVersion",
                BuildStreetName(version),
                BuildEvents(version),
                predecessors.Select(x => BuildLink(Mutations + "predecessor", version.State.Namespace, x)));

        public static XElement BuildUpdate(StreetNameVersionPart version, IEnumerable<StreetNameVersionLink> successors)
            => new XElement(Mutations + "updateStreetNameVersion",
                BuildStreetName(version),
                BuildEvents(version),
                successors.Select(x => BuildLink(Mutations + "successor", version.State.Namespace, x)));

        private static XElement BuildStreetName(StreetNameVersionPart version)
        {
            var state = version.State;

            return new XElement(Mutations + "streetName",
                new XAttribute("beginLifeSpanVersion", FormatDateTime(state.BeginLifeSpan)),
                version.EndLifeSpan is null ? null : new XAttribute("endLifeSpanVersion", FormatDateTime(version.EndLifeSpan.Value)),
                BuildIdentifier(Common + "code", FormatNamespace(state.Namespace), state.PersistentLocalId.ToString(CultureInfo.InvariantCulture), FormatVersion(state.VersionId)),
                GetNames(state).Select(x => new XElement(Common + "name",
                    new XElement(Common + "language", x.Language),
                    new XElement(Common + "spelling", x.Spelling))),
                new XElement(Common + "streetNameStatus",
                    new XElement(Common + "status", MapStatus(state.Status)),
                    new XElement(Common + "validFrom", FormatDateTime(state.ValidFrom)),
                    version.EndLifeSpan is null ? null : new XElement(Common + "validTo", FormatDateTime(version.EndLifeSpan.Value))),
                new XElement(Common + "type", "streetname"),
                GetHomonymAddition(state) is { } homonymAddition ? new XElement(Common + "homonymAddition", homonymAddition) : null,
                BuildIdentifier(Common + "isAssignedByMunicipality", MunicipalityNamespace, state.NisCode, VersionPlaceholder(MunicipalityPlaceholderEntity, state.NisCode)));
        }

        public const string MunicipalityPlaceholderEntity = "municipality";

        /// <summary>
        /// The version of an object owned by another registry, e.g. "{{municipality:11001}}".
        /// The daily service that combines the files replaces it with the version at the end of the day,
        /// with a text replace while it copies the files, and fails if a placeholder remains.
        /// </summary>
        public static string VersionPlaceholder(string entity, string objectIdentifier) => "{{" + entity + ":" + objectIdentifier + "}}";

        private static IEnumerable<XElement> BuildEvents(StreetNameVersionPart version)
            => version.Events.Select(x => new XElement(Mutations + "event",
                new XElement(Mutations + "date", FormatDateTime(x.Date)),
                new XElement(Mutations + "type", x.Type)));

        private static XElement BuildLink(XName name, string @namespace, StreetNameVersionLink link)
            => BuildIdentifier(name, FormatNamespace(@namespace), link.PersistentLocalId.ToString(CultureInfo.InvariantCulture), FormatVersion(link.VersionId));

        private static XElement BuildIdentifier(XName name, string @namespace, string objectIdentifier, string versionIdentifier)
            => new XElement(name,
                new XElement(Common + "namespace", @namespace),
                new XElement(Common + "objectIdentifier", objectIdentifier),
                new XElement(Common + "versionIdentifier", versionIdentifier));

        private static string MapStatus(StreetNameStatus status) => status switch
        {
            StreetNameStatus.Proposed => "proposed",
            StreetNameStatus.Current => "current",
            StreetNameStatus.Retired => "retired",
            StreetNameStatus.Rejected => "rejected",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

        /// <summary>
        /// The XSD allows only one homonym addition and no English.
        /// </summary>
        private static string? GetHomonymAddition(StreetNameVersionState state)
            => new[] { state.HomonymAdditionDutch, state.HomonymAdditionFrench, state.HomonymAdditionGerman }
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        private static IEnumerable<(string Language, string Spelling)> GetNames(StreetNameVersionState state)
        {
            if (!string.IsNullOrEmpty(state.NameDutch))
                yield return ("nl", state.NameDutch);

            if (!string.IsNullOrEmpty(state.NameFrench))
                yield return ("fr", state.NameFrench);

            if (!string.IsNullOrEmpty(state.NameGerman))
                yield return ("de", state.NameGerman);
        }

        private static string FormatNamespace(string @namespace) => @namespace.EndsWith('/') ? @namespace : $"{@namespace}/";

        /// <summary>
        /// Same as the versionIdentifier of the full download.
        /// </summary>
        public static string FormatVersion(DateTimeOffset version)
            => Instant.FromDateTimeOffset(version).ToBelgianDateTimeOffset().ToString("yyyy-MM-ddTHH:mm:ss%K", CultureInfo.InvariantCulture);

        private static string FormatDateTime(DateTimeOffset value)
            => value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }
}
