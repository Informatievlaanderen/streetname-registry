namespace StreetNameRegistry.Importer.Console.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;
    using Be.Vlaanderen.Basisregisters.Crab;
    using StreetName.Commands;

    internal class StreetNameMappings
    {
        public static ImportStreetNameFromCrab GetCommandFor(
            tblStraatNaam straatnaam,
            string primaryLanguage,
            string secondaryLanguage,
            string nisCode)
        {
            MapLogging.Log(".");

            return new ImportStreetNameFromCrab(
                new CrabStreetNameId(straatnaam.straatNaamId),
                new CrabMunicipalityId(straatnaam.gemeenteId),
                new NisCode(nisCode),
                new CrabStreetName(
                    straatnaam.straatNaam?.Trim(),
                    ParseLanguage(primaryLanguage)),
                new CrabStreetName(
                    straatnaam.straatNaamTweedeTaal?.Trim(),
                    ParseLanguage(secondaryLanguage)),
                new CrabTransStreetName(
                    straatnaam.transStraatNaam?.Trim(),
                    ParseLanguage(primaryLanguage)),
                new CrabTransStreetName(
                    straatnaam.transStraatNaamTweedeTaal?.Trim(),
                    ParseLanguage(secondaryLanguage)),
                ParseLanguage(primaryLanguage),
                ParseLanguage(secondaryLanguage),
                new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(
                    straatnaam.beginDatum.ToCrabLocalDateTime(),
                    straatnaam.eindDatum?.ToCrabLocalDateTime()),
                new CrabTimestamp(straatnaam.CrabTimestamp.ToCrabInstant()),
                new CrabOperator(straatnaam.Operator),
                ParseBewerking(straatnaam.Bewerking),
                ParseOrganisatie(straatnaam.Organisatie)
            );
        }

        public static IEnumerable<ImportStreetNameFromCrab> GetCommandsFor(
            IEnumerable<tblStraatNaam_hist> straatnaamHists,
            string primaryLanguage,
            string secondaryLanguage,
            string nisCode)
        {
            return straatnaamHists
                .Select(straatnaam =>
                {
                    MapLogging.Log(".");

                    return new ImportStreetNameFromCrab(
                        new CrabStreetNameId(straatnaam.straatNaamId.Value),
                        new CrabMunicipalityId(straatnaam.gemeenteId.Value),
                        new NisCode(nisCode),
                        new CrabStreetName(
                            straatnaam.straatNaam?.Trim(),
                            ParseLanguage(primaryLanguage)),
                        new CrabStreetName(
                            straatnaam.straatNaamTweedeTaal?.Trim(),
                            ParseLanguage(secondaryLanguage)),
                        new CrabTransStreetName(
                            straatnaam.transStraatNaam?.Trim(),
                            ParseLanguage(primaryLanguage)),
                        new CrabTransStreetName(
                            straatnaam.transStraatNaamTweedeTaal?.Trim(),
                            ParseLanguage(secondaryLanguage)),
                        ParseLanguage(primaryLanguage),
                        ParseLanguage(secondaryLanguage),
                        new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(
                            straatnaam.beginDatum?.ToCrabLocalDateTime(),
                            straatnaam.eindDatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(straatnaam.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(straatnaam.Operator),
                        ParseBewerking(straatnaam.Bewerking),
                        ParseOrganisatie(straatnaam.Organisatie)
                    );
                });
        }

        public static IEnumerable<ImportStreetNameStatusFromCrab> GetCommandsFor(IEnumerable<tblStraatnaamstatus> straatnaamstatuses)
        {
            return straatnaamstatuses
                .Select(straatnaamStatus =>
                {
                    MapLogging.Log(".");

                    return new ImportStreetNameStatusFromCrab(
                        new CrabStreetNameStatusId(straatnaamStatus.straatnaamstatusid),
                        new CrabStreetNameId(straatnaamStatus.straatnaamid),
                        ParseStatus(straatnaamStatus.StraatnaamStatus),
                        new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(
                            straatnaamStatus.begindatum.ToCrabLocalDateTime(),
                            straatnaamStatus.einddatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(straatnaamStatus.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(straatnaamStatus.Operator),
                        ParseBewerking(straatnaamStatus.Bewerking),
                        ParseOrganisatie(straatnaamStatus.Organisatie));
                });
        }

        public static IEnumerable<ImportStreetNameStatusFromCrab> GetCommandsFor(IEnumerable<tblStraatnaamstatus_hist> straatnaamstatusesHist)
        {
            return straatnaamstatusesHist
                .Select(straatnaamstatusHist =>
                {
                    MapLogging.Log(".");

                    return new ImportStreetNameStatusFromCrab(
                        new CrabStreetNameStatusId(straatnaamstatusHist.straatnaamstatusid.Value),
                        new CrabStreetNameId(straatnaamstatusHist.straatnaamid.Value),
                        ParseStatus(straatnaamstatusHist.StraatnaamStatus),
                        new Be.Vlaanderen.Basisregisters.Crab.CrabLifetime(
                            straatnaamstatusHist.begindatum?.ToCrabLocalDateTime(),
                            straatnaamstatusHist.einddatum?.ToCrabLocalDateTime()),
                        new CrabTimestamp(straatnaamstatusHist.CrabTimestamp.ToCrabInstant()),
                        new CrabOperator(straatnaamstatusHist.Operator),
                        ParseBewerking(straatnaamstatusHist.Bewerking),
                        ParseOrganisatie(straatnaamstatusHist.Organisatie));
                });
        }

        private static CrabStreetNameStatus ParseStatus(CrabStraatnaamstatusEnum straatnaamstatus)
        {
            if (straatnaamstatus.Code == CrabStraatnaamstatusEnum.Voorgesteld.Code)
                return CrabStreetNameStatus.Proposed;

            if (straatnaamstatus.Code == CrabStraatnaamstatusEnum.Gereserveerd.Code)
                return CrabStreetNameStatus.Reserved;

            if (straatnaamstatus.Code == CrabStraatnaamstatusEnum.InGebruik.Code)
                return CrabStreetNameStatus.InUse;

            if (straatnaamstatus.Code == CrabStraatnaamstatusEnum.BuitenGebruik.Code)
                return CrabStreetNameStatus.OutOfUse;

            if (straatnaamstatus.Code == CrabStraatnaamstatusEnum.Onbekend.Code)
                return CrabStreetNameStatus.Unknown;

            throw new Exception($"Onbekende straatnaam status {straatnaamstatus.Code}");
        }

        private static CrabLanguage? ParseLanguage(string taalCode)
        {
            if (string.IsNullOrWhiteSpace(taalCode))
                return null;

            switch (taalCode.ToLowerInvariant())
            {
                case "nl": return CrabLanguage.Dutch;
                case "fr": return CrabLanguage.French;
                case "de": return CrabLanguage.German;
                case "en": return CrabLanguage.English;
            }

            throw new Exception($"Onbekende taalcode {taalCode}.");
        }

        private static CrabModification? ParseBewerking(CrabBewerking bewerking)
        {
            if (bewerking == null)
                return null;

            if (bewerking.Code == CrabBewerking.Invoer.Code)
                return CrabModification.Insert;

            if (bewerking.Code == CrabBewerking.Correctie.Code)
                return CrabModification.Correction;

            if (bewerking.Code == CrabBewerking.Historering.Code)
                return CrabModification.Historize;

            if (bewerking.Code == CrabBewerking.Verwijdering.Code)
                return CrabModification.Delete;

            throw new Exception($"Onbekende bewerking {bewerking.Code}");
        }

        private static CrabOrganisation? ParseOrganisatie(CrabOrganisatieEnum organisatie)
        {
            if (organisatie == null)
                return null;

            if (CrabOrganisatieEnum.AKRED.Code == organisatie.Code)
                return CrabOrganisation.Akred;

            if (CrabOrganisatieEnum.Andere.Code == organisatie.Code)
                return CrabOrganisation.Other;

            if (CrabOrganisatieEnum.DePost.Code == organisatie.Code)
                return CrabOrganisation.DePost;

            if (CrabOrganisatieEnum.Gemeente.Code == organisatie.Code)
                return CrabOrganisation.Municipality;

            if (CrabOrganisatieEnum.NGI.Code == organisatie.Code)
                return CrabOrganisation.Ngi;

            if (CrabOrganisatieEnum.NavTeq.Code == organisatie.Code)
                return CrabOrganisation.NavTeq;

            if (CrabOrganisatieEnum.Rijksregister.Code == organisatie.Code)
                return CrabOrganisation.NationalRegister;

            if (CrabOrganisatieEnum.TeleAtlas.Code == organisatie.Code)
                return CrabOrganisation.TeleAtlas;

            if (CrabOrganisatieEnum.VKBO.Code == organisatie.Code)
                return CrabOrganisation.Vkbo;

            if (CrabOrganisatieEnum.VLM.Code == organisatie.Code)
                return CrabOrganisation.Vlm;

            throw new Exception($"Onbekende organisatie {organisatie.Code}");
        }
    }
}
