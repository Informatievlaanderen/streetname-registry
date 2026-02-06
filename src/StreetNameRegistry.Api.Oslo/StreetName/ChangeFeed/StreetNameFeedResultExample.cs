namespace StreetNameRegistry.Api.Oslo.StreetName.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class StreetNameFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _feedConfig;

        public StreetNameFeedResultExample(IOptions<ResponseOptions> feedConfig)
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
                                 "dataschema": "{{_feedConfig.StreetNameFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "StreetNameWasMigratedToMunicipality",
                                 "basisregisterscausationid": "b42dcc08-a41e-50d2-ab21-87f2be687e42",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/straatnaam/84008",
                                     "objectId": "84008",
                                     "naamruimte": "https://data.vlaanderen.be/id/straatnaam",
                                     "versieId": "2023-11-01T08:18:40.8661748+01:00",
                                     "nisCodes": [
                                         "52043"
                                     ],
                                     "attributen": [
                                         {
                                             "naam": "gemeente.id",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "https://data.vlaanderen.be/id/gemeente/52043"
                                         },
                                         {
                                             "naam": "straatnaamStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "voorgesteld"
                                         },
                                         {
                                             "naam": "straatnamen",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 {
                                                     "spelling": "Rue Jules Stracmans",
                                                     "taal": "fr"
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
