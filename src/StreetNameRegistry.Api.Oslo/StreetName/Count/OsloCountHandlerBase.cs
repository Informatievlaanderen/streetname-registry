namespace StreetNameRegistry.Api.Oslo.StreetName.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Projections.Legacy;
    using Projections.Syndication;

    public record OsloCountRequest(HttpRequest HttpRequest, LegacyContext LegacyContext, SyndicationContext SyndicationContext) : IRequest<IActionResult>;

    public abstract class OsloCountHandlerBase : IRequestHandler<OsloCountRequest, IActionResult>
    {
        public abstract Task<IActionResult> Handle(OsloCountRequest request, CancellationToken cancellationToken);
    }
}
