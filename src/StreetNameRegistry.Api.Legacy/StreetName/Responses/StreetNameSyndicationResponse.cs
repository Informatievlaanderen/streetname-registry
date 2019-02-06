namespace StreetNameRegistry.Api.Legacy.StreetName.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
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
                Published = streetName.RecordCreatedAt.ToDateTimeOffset(),
                LastUpdated = streetName.LastChangedOn.ToDateTimeOffset(),
                Description = BuildDescription(streetName, responseOptions.Value.Naamruimte)
            };

            // TODO: Hier moet prolly version nog ergens in
            if (streetName.OsloId.HasValue)
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.Naamruimte}/{streetName.OsloId.Value}"),
                        AtomLinkTypes.Related));

                item.AddLink(
                    new SyndicationLink(
                        new Uri(string.Format(responseOptions.Value.DetailUrl, streetName.OsloId.Value)),
                        AtomLinkTypes.Self));

                item.AddLink(
                    new SyndicationLink(
                            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", streetName.OsloId.Value)),
                            AtomLinkTypes.Alternate)
                        { MediaType = MediaTypeNames.Application.Xml });

                item.AddLink(
                    new SyndicationLink(
                            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", streetName.OsloId.Value)),
                            AtomLinkTypes.Alternate)
                        { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    "agentschap Informatie Vlaanderen",
                    "informatie.vlaanderen@vlaanderen.be",
                    AtomContributorTypes.Author));

            await writer.Write(new SyndicationContent(formatter.CreateContent(item)));

        }

        private static string BuildDescription(StreetNameSyndicationQueryResult streetName, string naamruimte)
        {
            var content = new StreetNameSyndicationContent(
                streetName.StreetNameId.Value,
                naamruimte,
                streetName.OsloId,
                streetName.Status,
                streetName.NisCode,
                streetName.NameDutch,
                streetName.NameFrench,
                streetName.NameGerman,
                streetName.NameEnglish,
                streetName.HomonymAdditionDutch,
                streetName.HomonymAdditionFrench,
                streetName.HomonymAdditionEnglish,
                streetName.HomonymAdditionGerman,
                streetName.IsComplete,
                streetName.LastChangedOn.ToBelgianDateTimeOffset(),
                streetName.Organisation,
                streetName.Plan);

            return content.ToXml();
        }
    }

    [DataContract(Name = "Straatnaam", Namespace = "")]
    public class StreetNameSyndicationContent : SyndicationContentBase
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
        public Identificator Identificator { get; set; }

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
            int? osloId,
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
            Plan? plan)
        {
            StreetNameId = streetNameId;
            NisCode = nisCode;
            Identificator = new Identificator(naamruimte, osloId?.ToString(CultureInfo.InvariantCulture), version);
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

            Provenance = new Provenance(organisation, plan);
        }
    }

    public class StreetNameSyndicationResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public StreetNameSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <id>https://basisregisters.vlaanderen/syndication/feed/streetname.atom</id>
  <title>Basisregisters Vlaanderen - Straatnaamregister</title>
  <subtitle>Basisregisters Vlaanderen stelt u in staat om alles te weten te komen rond: de Belgische gemeenten; de Belgische postcodes; de Vlaamse straatnamen; de Vlaamse adressen; de Vlaamse gebouwen en gebouweenheden; de Vlaamse percelen; de Vlaamse organisaties en organen; de Vlaamse dienstverlening.</subtitle>
  <generator uri=""https://basisregisters.vlaanderen"" version=""2.0.0.0"">Basisregisters Vlaanderen</generator>
  <rights>Copyright (c) 2017-2018, Informatie Vlaanderen</rights>
  <updated>2018-10-05T14:06:53Z</updated>
  <author>
    <name>agentschap Informatie Vlaanderen</name>
    <email>informatie.vlaanderen@vlaanderen.be</email>
  </author>
  <link href=""https://basisregisters.vlaanderen/syndication/feed/streetname.atom"" rel=""self"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/"" rel=""related"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/v1/feeds/straatnamen.atom?offset=100&limit=100"" rel=""next""/>
  <entry>
    <id>4</id>
    <title>StreetNameWasRegistered-4</title>
    <updated>2018-10-04T13:12:17Z</updated>
    <published>2018-10-04T13:12:17Z</published>
    <link href=""{_responseOptions.Naamruimte}/13023"" rel=""related"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/straatnamen/13023"" rel=""self"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/straatnamen/13023.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/straatnamen/13023.json"" rel=""alternate"" type=""application/json"" />
    <author>
      <name>agentschap Informatie Vlaanderen</name>
      <email>informatie.vlaanderen@vlaanderen.be</email>
    </author>
    <category term=""https://data.vlaanderen.be/ns/straatnaam"" />
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
                <Plan>Centrale bijhouding CRAB</Plan>
                </Creatie>
                </Straatnaam>
                ]]></content>
  </entry>
</feed>";
        }
    }
}
