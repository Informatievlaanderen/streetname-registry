namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Names : ICollection<StreetNameName>
    {
        private readonly List<StreetNameName> _names = [];

        public Names()
        { }

        public Names(IEnumerable<StreetNameName> streetNameNames)
        {
            _names = streetNameNames.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
        }

        public Names(IDictionary<Language, string> streetNameNames)
            : this(streetNameNames
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

            _names.Add(new StreetNameName(name, language));
        }

        public bool Remove(Language language)
        {
            var index = GetIndexByLanguage(language);
            if (index != -1)
            {
                _names.RemoveAt(index);
                return true;
            }

            return false;
        }

        private int GetIndexByLanguage(Language language)
            => _names.FindIndex(name => name.Language == language);

        public IEnumerator<StreetNameName> GetEnumerator()
        {
            return _names.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(StreetNameName item) => Add(item.Language, item.Name);

        public void Clear() => throw new NotImplementedException();

        public bool Contains(StreetNameName item) => _names.Contains(item);

        public void CopyTo(StreetNameName[] array, int arrayIndex) => _names.CopyTo(array, arrayIndex);

        public bool Remove(StreetNameName item) => Remove(item.Language);

        public int Count => _names.Count;
        public bool IsReadOnly => false;
    }
}
