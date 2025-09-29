namespace StreetNameRegistry.Api.Oslo.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Converters;
    using Elastic.Clients.Elasticsearch;
    using Elastic.Clients.Elasticsearch.Core.Search;
    using Elastic.Clients.Elasticsearch.QueryDsl;
    using List;
    using Projections.Elastic.StreetNameList;
    using StreetNameRegistry.Infrastructure.Elastic.Exceptions;
    using Name = StreetNameRegistry.Infrastructure.Elastic.Name;

    public interface IStreetNameApiElasticSearchClient
    {
        Task<StreetNameListResult> ListStreetNames(
            string? streetName,
            string? nisCode,
            string? municipalityName,
            string? status,
            bool? inFlemishRegion,
            int? from,
            int? size);

        Task<long> CountStreetNames(
            string? streetName,
            string? nisCode,
            string? municipalityName,
            string? status,
            bool? inFlemishRegion);
    }

    public sealed class StreetNameApiElasticSearchClient : IStreetNameApiElasticSearchClient
    {
        private const string Keyword = "keyword";
        private static readonly string NameSpelling = $"{ToCamelCase(nameof(Name.Spelling))}";

        public ElasticsearchClient ElasticsearchClient { get; }
        public string IndexAlias { get; }

        public StreetNameApiElasticSearchClient(ElasticsearchClient elasticsearchClient, string indexAlias)
        {
            ElasticsearchClient = elasticsearchClient;
            IndexAlias = indexAlias;
        }

        public async Task<StreetNameListResult> ListStreetNames(
            string? streetName,
            string? nisCode,
            string? municipalityName,
            string? status,
            bool? inFlemishRegion,
            int? from,
            int? size)
        {
            object? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && !Enum.TryParse(typeof(StraatnaamStatus), status, true, out parsedStatus))
            {
                return new StreetNameListResult(Enumerable.Empty<StreetNameListDocument>().ToList(), 0);
            }

            var searchResponse = await ElasticsearchClient.SearchAsync<StreetNameListDocument>(IndexAlias, descriptor =>
            {
                descriptor.Size(size);
                descriptor.From(from);
                descriptor.TrackTotalHits(new TrackHits(true));
                descriptor.Sort(new List<SortOptions>
                {
                    SortOptions.Field(new Field(ToCamelCase(nameof(StreetNameListDocument.StreetNamePersistentLocalId))),
                        new FieldSort { Order = SortOrder.Asc })
                });

                var query = FilterList(streetName, nisCode, municipalityName, status, inFlemishRegion, parsedStatus);
                if (query is not null)
                    descriptor.Query(query);
            });

            if (!searchResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to search for addresses", searchResponse.ElasticsearchServerError, searchResponse.DebugInformation);
            }

            return new StreetNameListResult(searchResponse.Documents, searchResponse.Total);
        }

        public async Task<long> CountStreetNames(
            string? streetName,
            string? nisCode,
            string? municipalityName,
            string? status,
            bool? inFlemishRegion)
        {
            object? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && !Enum.TryParse(typeof(StraatnaamStatus), status, true, out parsedStatus))
            {
                return 0L;
            }

            var countResponse = await ElasticsearchClient.CountAsync<StreetNameListDocument>(IndexAlias, descriptor =>
            {
                var query = FilterList(streetName, nisCode, municipalityName, status, inFlemishRegion, parsedStatus);
                if (query is not null)
                    descriptor.Query(query);
            });

            if (!countResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to search for addresses", countResponse.ElasticsearchServerError, countResponse.DebugInformation);
            }

            return countResponse.Count;
        }

        private static QueryDescriptor<StreetNameListDocument>? FilterList(
            string? streetName,
            string? nisCode,
            string? municipalityName,
            string? status,
            bool? inFlemishRegion,
            object? parsedStatus)
        {
            if (!string.IsNullOrEmpty(streetName)
                || !string.IsNullOrEmpty(nisCode)
                || !string.IsNullOrEmpty(municipalityName)
                || !string.IsNullOrEmpty(status)
                || inFlemishRegion.HasValue)
            {
                return new QueryDescriptor<StreetNameListDocument>()
                    .Bool(b =>
                    {
                        var filterConditions = new List<Action<QueryDescriptor<StreetNameListDocument>>>();

                        if (!string.IsNullOrEmpty(streetName))
                        {
                            var streetNameNames = $"{ToCamelCase(nameof(StreetNameListDocument.Names))}";
                            filterConditions.Add(m => m.Nested(t => t.Path(streetNameNames!)
                                .Query(q => q.Term(t2 => t2
                                    .Field($"{streetNameNames}.{NameSpelling}.{Keyword}"!)
                                    .Value(streetName)))));
                        }

                        if (!string.IsNullOrEmpty(nisCode))
                            filterConditions.Add(m => m.Term(t => t
                                .Field(
                                    $"{ToCamelCase(nameof(StreetNameListDocument.Municipality))}.{ToCamelCase(nameof(StreetNameListDocument.Municipality.NisCode))}"
                                    !)
                                .Value(nisCode)));

                        if (!string.IsNullOrEmpty(municipalityName))
                        {
                            var municipalityNames = $"{ToCamelCase(nameof(StreetNameListDocument.Municipality))}.{ToCamelCase(nameof(StreetNameListDocument.Municipality.Names))}";
                            filterConditions.Add(m => m.Nested(t => t.Path($"{municipalityNames}"!)
                                .Query(q => q.Term(t2 => t2
                                    .Field($"{municipalityNames}.{NameSpelling}.{Keyword}"!)
                                    .Value(municipalityName)))));
                        }

                        if (!string.IsNullOrEmpty(status))
                        {
                            var addressStatus = ((StraatnaamStatus)parsedStatus!).ConvertToMunicipalityStreetNameStatus();
                            filterConditions.Add(m => m.Term(t => t
                                .Field($"{ToCamelCase(nameof(StreetNameListDocument.Status))}"!)
                                .Value(Enum.GetName(addressStatus)!)));
                        }

                        if (inFlemishRegion.HasValue)
                        {
                            filterConditions.Add(m => m.Term(t => t
                                .Field($"{ToCamelCase(nameof(StreetNameListDocument.Municipality))}.{ToCamelCase(nameof(StreetNameListDocument.Municipality.IsInFlemishRegion))}"!)
                                .Value(inFlemishRegion.Value)));
                        }

                        b.Filter(filterConditions.ToArray());
                    });
            }

            return null;
        }

        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
                return str;

            var chars = str.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (i == 0 || i > 0 && char.IsUpper(chars[i]))
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }
                else
                {
                    break;
                }
            }

            return new string(chars);
        }
    }
}
