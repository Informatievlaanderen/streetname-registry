namespace StreetNameRegistry.Producer.Ldes
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Municipality;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class StreetNameLdes
    {
        private static readonly JObject Context = JObject.Parse(@"
{
  ""@version"": 1.1,
  ""@base"": ""https://basisregisters.vlaanderen.be/implementatiemodel/adressenregister"",
  ""@vocab"": ""#"",
  ""identificator"": ""@nest"",
  ""id"": ""@id"",
  ""versieId"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""naamruimte"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#naamruimte"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""objectId"": {
    ""@id"": ""https://data.vlaanderen.be/ns/generiek#lokaleIdentificator"",
    ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
  },
  ""gemeente"": {
    ""@id"": ""http://www.w3.org/ns/prov#wasAttributedTo"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/id/gemeente/"",
      ""objectId"": ""@id""
    }
  },
  ""straatnamen"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#Straatnaam"",
    ""@container"": ""@language""
  },
  ""homoniemToevoegingen"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#homoniemToevoeging"",
    ""@container"": ""@language""
  },
  ""straatnaamStatus"": {
    ""@id"": ""https://data.vlaanderen.be/ns/adres#Straatnaam.status"",
    ""@type"": ""@id"",
    ""@context"": {
      ""@base"": ""https://data.vlaanderen.be/doc/concept/straatnaamstatus/""
    }
  }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "Straatnaam";

        [JsonProperty("Identificator", Order = 2)]
        public StraatnaamIdentificator Identificator { get; private set; }

        [JsonProperty("Gemeente", Order = 3)]
        public GemeenteObjectId Gemeente { get; private set; }

        [JsonProperty("Straatnamen", Order = 4)]
        public Dictionary<string, string> Straatnamen { get; private set; }

        [JsonProperty("HomoniemToevoegingen", Order = 5)]
        public Dictionary<string, string> HomoniemToevoegingen { get; private set; }

        [JsonProperty("StraatnaamStatus", Order = 6)]
        public StraatnaamStatus StraatnaamStatus { get; private set; }

        [JsonProperty("IsVerwijderd", Order = 7)]
        public bool IsRemoved { get; private set; }

        public StreetNameLdes(StreetNameDetail streetName, string osloNamespace)
        {
            Identificator = new StraatnaamIdentificator(osloNamespace, streetName.StreetNamePersistentLocalId.ToString(), streetName.VersionTimestamp.ToBelgianDateTimeOffset());
            Gemeente = new GemeenteObjectId(streetName.NisCode);
            Straatnamen = new Dictionary<string, string>(
                new[]
                    {
                        ("nl", streetName.NameDutch),
                        ("fr", streetName.NameFrench),
                        ("de", streetName.NameGerman),
                        ("en", streetName.NameEnglish)
                    }
                    .Where(pair => !string.IsNullOrEmpty(pair.Item2))
                    .ToDictionary(pair => pair.Item1, pair => pair.Item2)!
            );
            HomoniemToevoegingen = new Dictionary<string, string>(
                new[]
                    {
                        ("nl", streetName.HomonymAdditionDutch),
                        ("fr", streetName.HomonymAdditionFrench),
                        ("de", streetName.HomonymAdditionGerman),
                        ("en", streetName.HomonymAdditionEnglish)
                    }
                    .Where(pair => !string.IsNullOrEmpty(pair.Item2))
                    .ToDictionary(pair => pair.Item1, pair => pair.Item2)!
            );
            StraatnaamStatus = streetName.Status.ConvertToStraatnaamStatus();
            IsRemoved = streetName.IsRemoved;
        }
    }

    public sealed class GemeenteObjectId
    {
        [JsonProperty("ObjectId")]
        public string ObjectId { get; private set; }

        public GemeenteObjectId(string objectId)
        {
            ObjectId = objectId;
        }
    }
}
