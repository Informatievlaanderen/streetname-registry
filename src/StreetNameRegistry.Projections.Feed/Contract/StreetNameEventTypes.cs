namespace StreetNameRegistry.Projections.Feed.Contract
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public static class StreetNameEventTypes
    {
        public const string CreateV1 = "basisregisters.streetname.create.v1";
        public const string UpdateV1 = "basisregisters.streetname.update.v1";
        public const string DeleteV1 = "basisregisters.streetname.delete.v1";
        public const string TransformV1 = "basisregisters.streetname.transform.v1";
    }

    public static class StreetNameAttributeNames
    {
        public const string MunicipalityId = "gemeente.id";
        public const string StatusName = "straatnaamStatus";
        public const string StreetNameNames = "straatnamen";
        public const string HomonymAdditions = "homoniemToevoegingen";
    }

    public sealed class StreetNameCloudTransformEvent
    {
        [JsonProperty("vanIds", Order = 0)]
        public required List<string> From { get; set; }

        [JsonProperty("naarIds", Order = 1)]
        public required List<string> To { get; set; }

        [JsonProperty("nisCodes", Order = 2)]
        public required List<string> NisCodes { get; set; }
    }
}
