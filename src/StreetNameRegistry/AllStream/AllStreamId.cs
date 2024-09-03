namespace StreetNameRegistry.AllStream
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class AllStreamId : ValueObject<AllStreamId>
    {
        public static readonly AllStreamId Instance = new();

        private readonly Guid _id = new("97dc0cc3-a598-44f6-9b95-b1a8e8f0bde1");

        private AllStreamId() { }

        protected override IEnumerable<object> Reflect()
        {
            yield return _id;
        }

        public override string ToString() => _id.ToString("D");
    }
}
