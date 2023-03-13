namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Exceptions;

    public sealed class StreetNameHomonymAddition : ValueObject<StreetNameHomonymAddition>
    {
        public string HomonymAddition { get; }
        public Language Language { get; }

        public StreetNameHomonymAddition(string homonymAddition, Language language)
        {
            if (string.IsNullOrWhiteSpace(homonymAddition))
            {
                throw new ArgumentNullException(nameof(homonymAddition), "Value cannot be null or empty.");
            }

            if (homonymAddition.Length > 20)
            {
                throw new HomonymAdditionMaxCharacterLengthExceededException(language, homonymAddition.Length);
            }

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
