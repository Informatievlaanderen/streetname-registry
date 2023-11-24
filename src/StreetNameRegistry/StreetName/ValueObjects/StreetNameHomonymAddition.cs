namespace StreetNameRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public sealed class StreetNameHomonymAddition : ValueObject<StreetNameHomonymAddition>
    {
        public string HomonymAddition { get; }
        public Language? Language { get; }

        public StreetNameHomonymAddition(string homonymAddition, Language? language)
        {
            HomonymAddition = homonymAddition;
            Language = language;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return HomonymAddition;
            yield return Language;
        }
    }
}
