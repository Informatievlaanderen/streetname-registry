namespace StreetNameRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class StreetNameName : ValueObject<StreetNameName>
    {
        public string Name { get; }

        public Language? Language { get; }

        public StreetNameName(string name, Language? language)
        {
            Name = name;
            Language = language;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return Name;
            yield return Language;
        }

        public override string ToString() => $"{Name} ({Language})";
    }
}
