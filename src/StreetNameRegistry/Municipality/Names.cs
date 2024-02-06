namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Names : List<StreetNameName>
    {
        public Names()
        { }

        public Names(IEnumerable<StreetNameName> streetNameNames)
            : base(streetNameNames.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        { }

        public Names(IDictionary<Language, string> streetNameNames)
            : base(streetNameNames
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => new StreetNameName(x.Value, x.Key)))
        { }

        public bool HasMatch(Language language, string name)
        {
            var nameToMatch = new StreetNameName(name, language);
            return this.Any(x => x == nameToMatch);
        }

        public bool HasExactMatch(Language language, string name)
        {
            return this.Any(x => x.Name == name && x.Language == language);
        }

        public bool HasLanguage(Language language)
            => this.Any(name => name.Language == language);

        public void AddOrUpdate(Language language, string name)
        {
            if (HasLanguage(language))
            {
                Update(language, name);
            }
            else
            {
                Add(language, name);
            }
        }

        public IDictionary<Language, string> ToDictionary() =>
            this.ToDictionary(
                x => x.Language,
                x => x.Name);

        private void Update(Language language, string name)
        {
            if (!HasLanguage(language))
            {
                throw new InvalidOperationException("Index out of range");
            }

            Remove(language);
            Add(language, name);
        }

        private void Add(Language language, string name)
        {
            if (HasLanguage(language))
            {
                throw new InvalidOperationException($"Already name present with language {language}");
            }

            Add(new StreetNameName(name, language));
        }

        public void Remove(Language language)
        {
            var index = GetIndexByLanguage(language);
            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        private int GetIndexByLanguage(Language language)
            => FindIndex(name => name.Language == language);
    }
}
