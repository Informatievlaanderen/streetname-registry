namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    public interface IOvoCodeWhiteList
    {
        Task<bool> IsWhiteListed(string ovoCode, CancellationToken cancellationToken);
    }

    public class OvoCodeWhiteList : IOvoCodeWhiteList
    {
        private readonly OvoCodeWhiteListOptions _ovoCodeWhiteList;

        public OvoCodeWhiteList(IOptions<OvoCodeWhiteListOptions> ovoCodeWhiteListOptions)
        {
            _ovoCodeWhiteList = ovoCodeWhiteListOptions.Value;
        }

        public Task<bool> IsWhiteListed(string ovoCode, CancellationToken cancellationToken)
        {
            return Task.FromResult(_ovoCodeWhiteList.OvoCodeWhiteList.Contains(ovoCode, StringComparer.InvariantCultureIgnoreCase));
        }
    }

    public class OvoCodeWhiteListOptions
    {
        public IList<string> OvoCodeWhiteList { get; set; }
    }
}
