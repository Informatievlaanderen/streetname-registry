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

    public class StreetNameSyndicationResponseExamples : IExamplesProvider<object>
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <id>https://api.basisregisters.vlaanderen.be/v1/feeds/straatnamen.atom</id>
  <title>Basisregisters Vlaanderen - feed 'straatnamen'</title>
  <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'straatnamen'.</subtitle>
  <generator>Basisregisters Vlaanderen</generator>
  <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
  <updated>2018-10-05T14:06:53Z</updated>
  <author>
    <name>agentschap Informatie Vlaanderen</name>
    <email>informatie.vlaanderen@vlaanderen.be</email>
  </author>
  <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/straatnamen"" rel=""self""/>
  <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/straatnamen.atom"" rel=""alternate"" type=""application/atom+xml""/>
  <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/straatnamen.xml"" rel=""alternate"" type=""application/xml""/>
  <link href=""https://docs.basisregisters.dev-vlaanderen.be/"" rel=""related""/>
  <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/straatnamen?from=100&limit=100"" rel=""next""/>
  <entry>
    <id>4</id>
    <title>StreetNameWasRegistered-4</title>
    <updated>2018-10-04T13:12:17Z</updated>
    <published>2018-10-04T13:12:17Z</published>
    <link href=""{_responseOptions.Naamruimte}/13023"" rel=""related"" />
    <author>
      <name>agentschap Informatie Vlaanderen</name>
    </author>
    <category term=""straatnamen"" />
    <content><![CDATA[
<Straatnaam xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                <Id>6e04b6ff-0c9c-5770-a2ae-a7deba165469</Id>
                <Identificator>
                <Id>http://data.vlaanderen.be/id/straatnaam/12023</Id>
                <Naamruimte>http://data.vlaanderen.be/id/straatnaam</Naamruimte>
                <ObjectId>12023</ObjectId>
                <VersieId>4</VersieId>
                </Identificator>
                <Straatnamen />
                <StraatnaamStatus i:nil=""true"" />
                <HomoniemToevoegingen />
                <IsCompleet>false</IsCompleet>
                <Creatie>
                <Organisatie>Gemeente</Organisatie>
                <Reden>Centrale bijhouding CRAB</Reden>
                </Creatie>
                </Straatnaam>
                ]]></content>
  </entry>
</feed>";
        }
    }
}
