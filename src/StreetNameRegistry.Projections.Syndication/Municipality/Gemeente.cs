namespace StreetNameRegistry.Projections.Syndication.Municipality
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;

    [DataContract(Name = "Gemeente", Namespace = "")]
    public sealed class Gemeente
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public GemeenteIdentificator Identificator { get; set; }

        [DataMember(Name = "OfficieleTalen", Order = 3)]
        public List<Taal> OfficialLanguages { get; set; }

        [DataMember(Name = "FaciliteitenTalen", Order = 4)]
        public List<Taal> FacilitiesLanguages { get; set; }

        [DataMember(Name = "Gemeentenamen", Order = 5)]
        public List<GeografischeNaam> Gemeentenamen { get; set; }

        [DataMember(Name = "GemeenteStatus", Order = 6)]
        public GemeenteStatus? GemeenteStatus { get; set; }

        public Gemeente()
        {
            Gemeentenamen = new List<GeografischeNaam>();
            OfficialLanguages = new List<Taal>();
            FacilitiesLanguages = new List<Taal>();
        }
    }
}
