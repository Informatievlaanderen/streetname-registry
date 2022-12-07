namespace StreetNameRegistry.Api.Legacy.StreetName.Sync
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Legacy;
    using Query;

    public sealed record SyndicationAtomContent(string Content);
    public sealed record SyndicationRequest(FilteringHeader<StreetNameSyndicationFilter?> Filter, SortingHeader Sorting, IPaginationRequest Pagination) : IRequest<SyndicationAtomContent>;

    public sealed class SyndicationHandler : IRequestHandler<SyndicationRequest, SyndicationAtomContent>
    {
        private readonly LegacyContext _legacyContext;
        private readonly IOptions<ResponseOptions> _responseOptions;
        private readonly IConfiguration _configuration;

        public SyndicationHandler(
            LegacyContext legacyContext,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            _legacyContext = legacyContext;
            _responseOptions = responseOptions;
            _configuration = configuration;
        }

        public async Task<SyndicationAtomContent> Handle(SyndicationRequest request, CancellationToken cancellationToken)
        {
            var lastFeedUpdate = await _legacyContext
                .StreetNameSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
            {
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }

            var pagedStreetNames =
                new StreetNameSyndicationQuery(_legacyContext, request.Filter.Filter?.Embed)
                    .Fetch(request.Filter, request.Sorting, request.Pagination);

            return new SyndicationAtomContent(await BuildAtomFeed(lastFeedUpdate, pagedStreetNames));
        }

        private async Task<string> BuildAtomFeed(
            DateTimeOffset lastFeedUpdate,
            PagedQueryable<StreetNameSyndicationQueryResult> pagedStreetNames)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            await using (var xmlWriter = XmlWriter.Create(sw,
                             new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = _configuration.GetSection("Syndication");
                var atomFeedConfig = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastFeedUpdate);

                await writer.WriteDefaultMetadata(atomFeedConfig);

                var streetNames = pagedStreetNames.Items.ToList();

                var nextFrom = streetNames.Any()
                    ? streetNames.Max(s => s.Position) + 1
                    : (long?)null;

                var nextUri = BuildNextSyncUri(pagedStreetNames.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                {
                    await writer.Write(new SyndicationLink(nextUri, GrArAtomLinkTypes.Next));
                }

                foreach (var streetName in streetNames)
                {
                    await writer.WriteStreetName(_responseOptions, formatter, syndicationConfiguration["Category"], streetName);
                }

                // if we use await flush gets called sometimes after xmlWriter is disposed...
                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri? BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from, limit))
                : null;
        }
    }
}
