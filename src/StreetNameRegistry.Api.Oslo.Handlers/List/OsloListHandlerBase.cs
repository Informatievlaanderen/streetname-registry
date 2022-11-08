namespace StreetNameRegistry.Api.Oslo.Handlers.List
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public record OsloListRequest(HttpRequest HttpRequest, HttpResponse HttpResponse, LegacyContext LegacyContext, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions) : IRequest<IActionResult>;

    public abstract class OsloListHandlerBase : IRequestHandler<OsloListRequest, IActionResult>
    {
        protected static Uri? BuildNextUri(PaginationInfo paginationInfo, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }

        public abstract Task<IActionResult> Handle(OsloListRequest request, CancellationToken cancellationToken);
    }
}
