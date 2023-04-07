namespace StreetNameRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class NisCodeExtensions
    {
        private static List<string> s_whiteList = new List<string>
        {
            // add whitelisted niscodes
        };

        public static bool IsValidFor(this string? nisCodeInClaim, string? nisCodeInRequest)
        {
            // check whitelist
            return s_whiteList.Any(x => x.Equals(nisCodeInRequest, StringComparison.InvariantCultureIgnoreCase))
                || nisCodeInClaim.Equals(nisCodeInRequest, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
