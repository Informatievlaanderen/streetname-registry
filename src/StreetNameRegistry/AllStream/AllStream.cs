namespace StreetNameRegistry.AllStream
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;
    using Municipality;

    public sealed class AllStream : AggregateRootEntity
    {
        public void CreateOsloSnapshots(IReadOnlyList<PersistentLocalId> buildingUnitPersistentLocalIds)
        {
            ApplyChange(new StreetNameOsloSnapshotsWereRequested(buildingUnitPersistentLocalIds));
        }
    }
}
