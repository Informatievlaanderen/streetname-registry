namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using NisCodeService.Abstractions;

    public class NisCodeAuthorizer<T> : INisCodeAuthorizer<T>
    {
        private readonly INisCodeFinder<T> _nisCodeFinder;
        private readonly INisCodeService _nisCodeService;
        private readonly IOvoCodeWhiteList _ovoCodeWhiteList;

        public NisCodeAuthorizer(
            INisCodeFinder<T> nisCodeFinder,
            INisCodeService nisCodeService,
            IOvoCodeWhiteList ovoCodeWhiteList)
        {
            _nisCodeFinder = nisCodeFinder;
            _nisCodeService = nisCodeService;
            _ovoCodeWhiteList = ovoCodeWhiteList;
        }

        public async Task<bool> IsNotAuthorized(HttpContext httpContext, T id, CancellationToken ct)
        {
            var ovoCodeClaim = httpContext.User.FindFirst(AcmIdmClaimTypes.VoOrgCode);

            if (ovoCodeClaim is null)
            {
                return true;
            }

            if (await _ovoCodeWhiteList.IsWhiteListed(ovoCodeClaim.Value, ct))
            {
                return false;
            }

            var requestNisCode = await _nisCodeService.Get(ovoCodeClaim.Value, ct);
            var streetNameNisCode = await _nisCodeFinder.FindAsync(id, ct);

            return string.IsNullOrEmpty(requestNisCode) || requestNisCode != streetNameNisCode;
        }
    }
}
