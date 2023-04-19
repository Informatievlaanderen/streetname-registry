namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface INisCodeAuthorizer<T>
    {
        Task<bool> IsNotAuthorized(HttpContext httpContext, T id, CancellationToken ct);
    }
}
