namespace StreetNameRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;

    public class CreateOsloSnapshotsRequest
    {
        public List<int> PersistentLocalIds { get; set; }

        public string Reden { get; set; }
    }
}
