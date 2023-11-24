namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "HernoemenStraatnaam", Namespace = "")]
    public sealed class RenameStreetNameRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de doelstraatnaam.
        /// </summary>
        [DataMember(Name = "DoelStraatnaamId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string DoelStraatnaamId { get; set; }
    }
}
