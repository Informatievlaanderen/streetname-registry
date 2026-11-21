namespace StreetNameRegistry.Tests.Bosa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Projections.Bosa;

    /// <summary>
    /// Validates against Mutations.xsd (with BeStCommon.xsd).
    /// </summary>
    public static class BosaXsd
    {
        private static readonly XNamespace Mutations = StreetNameMutationXml.Mutations;
        private static readonly Lazy<XmlSchemaSet> Schemas = new Lazy<XmlSchemaSet>(Load);

        /// <summary>
        /// Combines an add and update file into a mutations document, like the daily service does, and validates it.
        /// </summary>
        public static IReadOnlyCollection<string> ValidateMutations(XDocument? add, XDocument? update)
        {
            var document = new XDocument(
                new XElement(Mutations + "mutationsResponseBySource",
                    new XElement(Mutations + "mutations",
                        new XElement(Mutations + "deltaBeginDateTime", "2026-10-20T20:00:00Z"),
                        new XElement(Mutations + "deltaEndDateTime", "2026-10-21T20:00:00Z"),
                        new XElement(Mutations + "source", "flanders"),
                        add?.Root is { } addRoot && addRoot.HasElements ? new XElement(addRoot) : null,
                        update?.Root is { } updateRoot && updateRoot.HasElements ? new XElement(updateRoot) : null)));

            var errors = new List<string>();
            document.Validate(Schemas.Value, (_, e) => errors.Add($"{e.Severity}: {e.Message}"));
            return errors;
        }

        private static XmlSchemaSet Load()
        {
            // The resolver loads the imported BeStCommon.xsd.
            var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };

            using var reader = XmlReader.Create(Path.Combine(AppContext.BaseDirectory, "Bosa", "Xsd", "Mutations.xsd"));
            schemas.Add(null, reader);
            schemas.Compile();

            return schemas;
        }
    }
}
