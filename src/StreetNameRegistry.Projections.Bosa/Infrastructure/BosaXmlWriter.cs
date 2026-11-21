namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public interface IBosaXmlWriter
    {
        void Write(Stream stream, BosaChangeKind kind, IEnumerable<XElement> elements);
    }

    /// <summary>
    /// Writes the elements of one entity as an add or update element of Mutations.xsd.
    /// The daily service merges the children of these files per entity, in the order the XSD requires
    /// (adds: municipality, part of municipality, postal info, street name, address; updates: the reverse),
    /// into the add and update of mutationsResponseBySource/mutations.
    /// </summary>
    public sealed class BosaXmlWriter : IBosaXmlWriter
    {
        private static readonly XmlWriterSettings Settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            CloseOutput = false
        };

        public void Write(Stream stream, BosaChangeKind kind, IEnumerable<XElement> elements)
        {
            var root = new XElement(
                StreetNameMutationXml.Mutations + (kind == BosaChangeKind.Add ? "add" : "update"),
                // Same prefixes as the mutations document, so the daily service can copy the children as text.
                new XAttribute(XNamespace.Xmlns + "tns", StreetNameMutationXml.Mutations),
                new XAttribute(XNamespace.Xmlns + "com", StreetNameMutationXml.Common),
                elements);

            using var writer = XmlWriter.Create(stream, Settings);
            new XDocument(root).Save(writer);
        }
    }
}
