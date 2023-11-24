namespace StreetNameRegistry.StreetName.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [Obsolete("Interface for legacy events, use IHasPersistentLocalId instead.")]
    public interface IHasStreetNameId : IMessage
    {
        Guid StreetNameId { get; }
    }
}
