namespace StreetNameRegistry.Api.Oslo.StreetName.List
{
    using System.Collections.Generic;
    using Projections.Elastic.StreetNameList;

    public sealed class StreetNameListResult
    {
        public IReadOnlyCollection<StreetNameListDocument> StreetNames { get; }
        public long Total { get; }

        public StreetNameListResult(IReadOnlyCollection<StreetNameListDocument> streetNames, long total)
        {
            StreetNames = streetNames;
            Total = total;
        }

        public static StreetNameListResult Empty => new StreetNameListResult(new List<StreetNameListDocument>(), 0);
    }
}
