namespace StreetNameRegistry.Projections.Elastic.StreetNameList
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Elastic.Clients.Elasticsearch;
    using global::Elastic.Clients.Elasticsearch.Analysis;
    using global::Elastic.Clients.Elasticsearch.IndexManagement;
    using global::Elastic.Clients.Elasticsearch.Mapping;
    using Microsoft.Extensions.Configuration;
    using StreetNameRegistry.Infrastructure.Elastic;
    using StreetNameRegistry.Infrastructure.Elastic.Exceptions;
    using ExistsRequest = global::Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest;

    public sealed class StreetNameListElasticIndex : ElasticIndexBase
    {
        public const string StreetNameListNormalizer = "StreetNameListNormalizer";
        public const string StreetNameListIndexAnalyzer = "StreetNameListIndexAnalyzer";

        public StreetNameListElasticIndex(
            ElasticsearchClient client,
            IConfiguration configuration)
            : this(client, ElasticIndexOptions.LoadFromConfiguration(configuration.GetSection("Elastic"), indexNameKey: "ListIndexName", indexAliasKey: "ListIndexAlias"))
        { }

        public StreetNameListElasticIndex(
            ElasticsearchClient client,
            ElasticIndexOptions options)
            :base(client, options)
        { }

        public override async Task CreateIndexIfNotExist(CancellationToken ct)
        {
            var indexName = Indices.Index(IndexName);
            var response = await Client.Indices.ExistsAsync(new ExistsRequest(indexName), ct);
            if (response.Exists)
                return;

            var createResponse = await Client.Indices.CreateAsync<StreetNameListDocument>(indexName, c =>
            {
                c.Settings(x => x
                    .MaxResultWindow(1_000_500) // Linked to public-api offset of 1_000_000 + limit of 500
                    .Sort(s => s
                        .Field(Fields.FromExpression((StreetNameListDocument d) => d.StreetNamePersistentLocalId))
                        .Order([SegmentSortOrder.Asc]))
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .PatternReplace("dot_replace", prcf => prcf.Pattern("\\.").Replacement(""))
                            .PatternReplace("underscore_replace", prcf => prcf.Pattern("_").Replacement(" "))
                            .PatternReplace("quote_replace", prcf => prcf.Pattern("\'").Replacement(""))
                            .PatternReplace("hyphen_replace", prcf => prcf.Pattern("-").Replacement(" "))
                        )
                        .Normalizers(descriptor =>
                        {
                            AddStreetNameListNormalizer(descriptor);
                        })
                        .Analyzers(descriptor =>
                        {
                            AddStreetNameListIndexAnalyzer(descriptor);
                        })
                    )
                );

                c.Mappings(map => map
                    .Properties(p => p
                        .IntegerNumber(x => x.StreetNamePersistentLocalId)
                        .Object(x => x.Municipality, objConfig => objConfig
                            .Properties(obj =>
                            {
                                obj
                                    .Keyword(x => x.Municipality.NisCode)
                                    .Nested("names", ConfigureNames())
                                    .Keyword(x => x.Municipality.PrimaryLanguage)
                                    .Boolean(x => x.Municipality.IsInFlemishRegion)
                                    ;
                            })
                        )
                        .Nested("names", ConfigureNames())
                        .Nested("homonymAdditions", ConfigureNames())
                        .Keyword(x => x.Status)
                        .Date(x => x.VersionTimestamp)
                    ));
            }, ct);

            if (!createResponse.Acknowledged || !createResponse.IsValidResponse)
            {
                throw new ElasticsearchClientException("Failed to create an index", createResponse.ElasticsearchServerError, createResponse.DebugInformation);
            }
        }

        private Action<NestedPropertyDescriptor<StreetNameListDocument>> ConfigureNames(string analyzer = StreetNameListIndexAnalyzer)
        {
            return n => n
                .Properties(np => np
                    .Text("spelling", new TextProperty
                    {
                        Fields = new Properties
                        {
                            { "keyword", new KeywordProperty { IgnoreAbove = 256, Normalizer = StreetNameListNormalizer } }
                        },
                        Analyzer = analyzer,
                        SearchAnalyzer = analyzer
                    })
                    .Keyword("language")
                );
        }

        private static void AddStreetNameListNormalizer(NormalizersDescriptor normalizersDescriptor) =>
            normalizersDescriptor.Custom(StreetNameListNormalizer, ca => ca
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding", "trim"]));

        private static void AddStreetNameListIndexAnalyzer(AnalyzersDescriptor analyzersDescriptor) =>
            analyzersDescriptor.Custom(StreetNameListIndexAnalyzer, ca => ca
                .Tokenizer("standard")
                .CharFilter(["underscore_replace", "dot_replace", "quote_replace", "hyphen_replace"])
                .Filter(["lowercase", "asciifolding"])
            );
    }
}
