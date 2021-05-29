namespace StreetNameRegistry.Api.Legacy.StreetName.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class StreetNameSyndicationResponse
    {
        public static async Task WriteStreetName(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category,
            StreetNameSyndicationQueryResult streetName)
        {
            var item = new SyndicationItem
            {
                Id = streetName.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{streetName.ChangeType}-{streetName.Position}",
                Published = streetName.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = streetName.LastChangedOn.ToBelgianDateTimeOffset(),
                Description = BuildDescription(streetName, responseOptions.Value.Naamruimte)
            };

            if (streetName.PersistentLocalId.HasValue)
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.Naamruimte}/{streetName.PersistentLocalId.Value}"),
                        AtomLinkTypes.Related));

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format(responseOptions.Value.DetailUrl, streetName.PersistentLocalId.Value)),
                //        AtomLinkTypes.Self));

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", streetName.PersistentLocalId.Value)),
                //            AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Xml });

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", streetName.PersistentLocalId.Value)),
                //            AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    streetName.Organisation == null ? Organisation.Unknown.ToName() : streetName.Organisation.Value.ToName(),
                    string.Empty,
                    AtomContributorTypes.Author));

            await writer.Write(item);
        }

        private static string BuildDescription(StreetNameSyndicationQueryResult streetName, string naamruimte)
        {
            if (!streetName.ContainsEvent && !streetName.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();

            if (streetName.ContainsObject)
                content.Object = new StreetNameSyndicationContent(
                    streetName.StreetNameId.Value,
                    naamruimte,
                    streetName.PersistentLocalId,
                    streetName.Status,
                    streetName.NisCode,
                    streetName.NameDutch,
                    streetName.NameFrench,
                    streetName.NameGerman,
                    streetName.NameEnglish,
                    streetName.HomonymAdditionDutch,
                    streetName.HomonymAdditionFrench,
                    streetName.HomonymAdditionGerman,
                    streetName.HomonymAdditionEnglish,
                    streetName.IsComplete,
                    streetName.LastChangedOn.ToBelgianDateTimeOffset(),
                    streetName.Organisation,
                    streetName.Reason);

            if (streetName.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(streetName.EventDataAsXml);
                content.Event = doc.DocumentElement;
            }

            return content.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public StreetNameSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "Straatnaam", Namespace = "")]
    public class StreetNameSyndicationContent
    {
        /// <summary>
        /// De technische id van de straatnaam.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid StreetNameId { get; set; }

        /// <summary>
        /// De identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public StraatnaamIdentificator Identificator { get; set; }

        /// <summary>
        /// De officiële namen van de straatnaam.
        /// </summary>
        [DataMember(Name = "Straatnamen", Order = 3)]
        public List<GeografischeNaam> StreetNames { get; set; }

        /// <summary>
        /// De huidige fase in het leven van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamStatus", Order = 4)]
        public StraatnaamStatus? StreetNameStatus { get; set; }

        /// <summary>
        /// De homoniem-toevoegingen aan de straatnaam in verschillende talen.
        /// </summary>
        [DataMember(Name = "HomoniemToevoegingen", Order = 5)]
        public List<GeografischeNaam> HomonymAdditions { get; set; }

        /// <summary>
        /// De NisCode van de gerelateerde gemeente.
        /// </summary>
        [DataMember(Name = "NisCode", Order = 6)]
        public string NisCode { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 7)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 8)]
        public Provenance Provenance { get; set; }

        public StreetNameSyndicationContent(
            Guid streetNameId,
            string naamruimte,
            int? persistentLocalId,
            StreetNameStatus? status,
            string nisCode,
            string nameDutch,
            string nameFrench,
            string nameGerman,
            string nameEnglish,
            string homonymAdditionDutch,
            string homonymAdditionFrench,
            string homonymAdditionGerman,
            string homonymAdditionEnglish,
            bool isComplete,
            DateTimeOffset version,
            Organisation? organisation,
            string reason)
        {
            StreetNameId = streetNameId;
            NisCode = nisCode;
            Identificator = new StraatnaamIdentificator(naamruimte, persistentLocalId?.ToString(CultureInfo.InvariantCulture), version);
            StreetNameStatus = status?.ConvertFromStreetNameStatus();
            IsComplete = isComplete;

            var straatnamen = new List<GeografischeNaam>
            {
                new GeografischeNaam(nameDutch, Taal.NL),
                new GeografischeNaam(nameFrench, Taal.FR),
                new GeografischeNaam(nameGerman, Taal.DE),
                new GeografischeNaam(nameEnglish, Taal.EN),
            };

            StreetNames = straatnamen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();

            var homoniemToevoegingen = new List<GeografischeNaam>
            {
                new GeografischeNaam(homonymAdditionDutch, Taal.NL),
                new GeografischeNaam(homonymAdditionFrench, Taal.FR),
                new GeografischeNaam(homonymAdditionGerman, Taal.DE),
                new GeografischeNaam(homonymAdditionEnglish, Taal.EN),
            };

            HomonymAdditions = homoniemToevoegingen.Where(x => !string.IsNullOrEmpty(x.Spelling)).ToList();

            Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    public class StreetNameSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        private const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'straatnamen'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'straatnamen'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.15.0"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-11-12T09:25:05Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen?from=2&amp;limit=100&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>StreetNameWasRegistered-0</title>
        <updated>2002-11-21T11:23:45+01:00</updated>
        <published>2002-11-21T11:23:45+01:00</published>
        <author>
            <name>Vlaamse Landmaatschappij</name>
        </author>
        <category term=""straatnamen"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><StreetNameWasRegistered><StreetNameId>2a2f28ac-f084-5404-a229-434bd194f213</StreetNameId><MunicipalityId>ca6a4d57-e46d-571f-98df-e64c0e5fa3da</MunicipalityId><NisCode>62022</NisCode><Provenance><Timestamp>2002-11-21T10:23:45Z</Timestamp><Organisation>Vlm</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </StreetNameWasRegistered>
  </Event><Object><Id>2a2f28ac-f084-5404-a229-434bd194f213</Id><Identificator><Id>https://data.vlaanderen.be/id/straatnaam/</Id><Naamruimte>https://data.vlaanderen.be/id/straatnaam</Naamruimte><ObjectId i:nil=""true"" /><VersieId>2002-11-21T11:23:45+01:00</VersieId></Identificator><Straatnamen /><StraatnaamStatus i:nil=""true"" /><HomoniemToevoegingen /><NisCode>62022</NisCode><IsCompleet>false</IsCompleet><Creatie><Tijdstip>2002-11-21T11:23:45+01:00</Tijdstip><Organisatie>Vlaamse Landmaatschappij</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>1</id>
    <title>StreetNameWasNamed-1</title>
    <updated>2002-11-21T11:23:45+01:00</updated>
    <published>2002-11-21T11:23:45+01:00</published>
    <author>
        <name>Vlaamse Landmaatschappij</name>
    </author>
    <category term=""straatnamen"" />
    <content>
        <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><StreetNameWasNamed><StreetNameId>2a2f28ac-f084-5404-a229-434bd194f213</StreetNameId><Name>Drève de Méhagne</Name><Language>French</Language><Provenance><Timestamp>2002-11-21T10:23:45Z</Timestamp><Organisation>Vlm</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </StreetNameWasNamed>
  </Event><Object><Id>2a2f28ac-f084-5404-a229-434bd194f213</Id><Identificator><Id>https://data.vlaanderen.be/id/straatnaam/</Id><Naamruimte>https://data.vlaanderen.be/id/straatnaam</Naamruimte><ObjectId i:nil=""true"" /><VersieId>2002-11-21T11:23:45+01:00</VersieId></Identificator><Straatnamen><GeografischeNaam><Spelling>Drève de Méhagne</Spelling><Taal>fr</Taal></GeografischeNaam>
    </Straatnamen><StraatnaamStatus i:nil=""true"" /><HomoniemToevoegingen /><NisCode>62022</NisCode><IsCompleet>false</IsCompleet><Creatie><Tijdstip>2002-11-21T11:23:45+01:00</Tijdstip><Organisatie>Vlaamse Landmaatschappij</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
</feed>";

        public XmlElement GetExamples()
        {
            var example = new XmlDocument();
            example.LoadXml(RawXml);
            return example.DocumentElement;
        }
    }
}
