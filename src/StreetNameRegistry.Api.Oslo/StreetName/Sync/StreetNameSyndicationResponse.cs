namespace StreetNameRegistry.Api.Oslo.StreetName.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Converters;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Query;
    using StreetNameRegistry.Municipality;
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
                    streetName.StreetNameId.HasValue ? streetName.StreetNameId.Value.ToString("D") : streetName.PersistentLocalId.ToString(),
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
        /// De technische id van de aggregate.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string Id { get; set; }

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
            string id,
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
            Id = id;
            NisCode = nisCode;
            Identificator = new StraatnaamIdentificator(naamruimte, persistentLocalId?.ToString(CultureInfo.InvariantCulture), version);
            StreetNameStatus = status?.ConvertFromMunicipalityStreetNameStatus();
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
    <id>https://api.basisregisters.vlaanderen.be/v2/feeds/straatnamen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'straatnamen'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'straatnamen'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.15.0"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-11-12T09:25:05Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/straatnamen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/straatnamen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/straatnamen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/straatnamen?from=2&amp;limit=100&amp;embed=event,object"" rel=""next"" />
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
<entry>
    <id>1913485</id>
    <title>StreetNameWasMigratedToMunicipality-1913485</title>
    <updated>2023-11-01T08:18:40+01:00</updated>
    <published>2023-11-01T08:18:40+01:00</published>
    <link href=""https://data.vlaanderen.be/id/straatnaam/84008"" rel=""related"" />
    <author>
      <name>Digitaal Vlaanderen</name>
    </author>
    <category term=""straatnamen"" />
    <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <Event>
    <StreetNameWasMigratedToMunicipality>
      <MunicipalityId>6265de0b-96c5-5a14-aa21-3485e3be8bc5</MunicipalityId>
      <NisCode>52043</NisCode>
      <StreetNameId>c76e4103-d8d8-5037-af54-bc62d45dd626</StreetNameId>
      <PersistentLocalId>84008</PersistentLocalId>
      <Status>Current</Status>
      <PrimaryLanguage>French</PrimaryLanguage>
      <SecondaryLanguage />
      <Names>
        <Names_0>
          <Key>French</Key>
          <Value>Rue Jules Stracmans</Value>
        </Names_0>
      </Names>
      <HomonymAdditions />
      <IsCompleted>true</IsCompleted>
      <IsRemoved>false</IsRemoved>
      <Provenance>
        <Timestamp>2023-11-01T07:18:40Z</Timestamp>
        <Organisation>DigitaalVlaanderen</Organisation>
        <Reason>Migrate StreetName aggregate to Municipality.</Reason>
      </Provenance>
    </StreetNameWasMigratedToMunicipality>
  </Event>
  <Object>
    <Id>84008</Id>
    <Identificator>
      <Id>https://data.vlaanderen.be/id/straatnaam/84008</Id>
      <Naamruimte>https://data.vlaanderen.be/id/straatnaam</Naamruimte>
      <ObjectId>84008</ObjectId>
      <VersieId>2023-11-01T08:18:40+01:00</VersieId>
    </Identificator>
    <Straatnamen>
      <GeografischeNaam>
        <Spelling>Rue Jules Stracmans</Spelling>
        <Taal>fr</Taal>
      </GeografischeNaam>
    </Straatnamen>
    <StraatnaamStatus>InGebruik</StraatnaamStatus>
    <HomoniemToevoegingen />
    <NisCode>52043</NisCode>
    <IsCompleet>true</IsCompleet>
    <Creatie>
      <Tijdstip>2023-11-01T08:18:40+01:00</Tijdstip>
      <Organisatie>Digitaal Vlaanderen</Organisatie>
      <Reden>Migrate StreetName aggregate to Municipality.</Reden>
    </Creatie>
  </Object>
</Content>]]></content>
</entry>
<entry>
    <id>2233010</id>
    <title>StreetNameWasProposedV2-2233010</title>
    <updated>2024-08-13T15:00:42+02:00</updated>
    <published>2024-08-13T15:00:42+02:00</published>
    <link href=""https://data.vlaanderen.be/id/straatnaam/229009"" rel=""related"" />
    <author>
      <name>Andere</name>
    </author>
    <category term=""straatnamen"" />
    <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <Event>
    <StreetNameWasProposedV2>
      <MunicipalityId>c91136be-9b6c-5cab-8921-e8fc5e784600</MunicipalityId>
      <NisCode>24059</NisCode>
      <StreetNameNames>
        <StreetNameNames_0>
          <Key>Dutch</Key>
          <Value>Kruiske</Value>
        </StreetNameNames_0>
      </StreetNameNames>
      <PersistentLocalId>229009</PersistentLocalId>
      <Provenance>
        <Timestamp>2024-08-13T13:00:42Z</Timestamp>
        <Organisation>Other</Organisation>
        <Reason>
        </Reason>
      </Provenance>
    </StreetNameWasProposedV2>
  </Event>
  <Object>
    <Id>229009</Id>
    <Identificator>
      <Id>https://data.vlaanderen.be/id/straatnaam/229009</Id>
      <Naamruimte>https://data.vlaanderen.be/id/straatnaam</Naamruimte>
      <ObjectId>229009</ObjectId>
      <VersieId>2024-08-13T15:00:42+02:00</VersieId>
    </Identificator>
    <Straatnamen>
      <GeografischeNaam>
        <Spelling>Kruiske</Spelling>
        <Taal>nl</Taal>
      </GeografischeNaam>
    </Straatnamen>
    <StraatnaamStatus>Voorgesteld</StraatnaamStatus>
    <HomoniemToevoegingen />
    <NisCode>24059</NisCode>
    <IsCompleet>true</IsCompleet>
    <Creatie>
      <Tijdstip>2024-08-13T15:00:42+02:00</Tijdstip>
      <Organisatie>Andere</Organisatie>
      <Reden />
    </Creatie>
  </Object>
</Content>]]></content>
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
