namespace StreetNameRegistry.Api.Oslo.StreetName.V3.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class StreetNameFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptionsV3 _feedConfig;

        public StreetNameFeedResultExample(IOptions<ResponseOptionsV3> feedConfig)
        {
            _feedConfig = feedConfig.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                             {
                                 "specversion": "1.0",
                                 "id": "1",
                                 "time": "2023-11-01T08:18:40.8661748+01:00",
                                 "type": "basisregisters.streetname.create.v1",
                                 "source": "{{_feedConfig.StreetNameFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "subject": "https://data.vlaanderen.be/id/straatnaam/84008",
                                 "dataschema": "{{_feedConfig.StreetNameFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "StreetNameWasMigratedToMunicipality",
                                 "basisregisterscausationid": "b42dcc08-a41e-50d2-ab21-87f2be687e42",
                                 "data": {
                                     "objectId": "84008",
                                     "naamruimte": "https://data.vlaanderen.be/id/straatnaam",
                                     "versieId": "2023-11-01T08:18:40.8661748+01:00",
                                     "nisCodes": [
                                         "52043"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "isToegekendDoor",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/gemeente/52043"
                                         },
                                         {
                                             "naam": "status",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/concept/straatnaamstatus/voorgesteld"
                                         },
                                         {
                                             "naam": "straatnaam",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "@value": "Rue Jules Stracmans",
                                                     "@language": "fr"
                                                 }
                                             ]
                                         }
                                     ]
                                 }
                             }
                         ]
                         """;
            return JArray.Parse(json);
        }
    }
}
