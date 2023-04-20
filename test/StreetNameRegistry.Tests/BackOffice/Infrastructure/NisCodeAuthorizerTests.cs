namespace StreetNameRegistry.Tests.BackOffice.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Municipality;
    using NisCodeService.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization;
    using Xunit;

    public class NisCodeAuthorizerTests
    {
        private readonly NisCodeAuthorizer<PersistentLocalId> _authorizer;
        private readonly Mock<INisCodeFinder<PersistentLocalId>> _nisCodeFinder;
        private readonly Mock<INisCodeService> _nisCodeService;
        private readonly Mock<IOvoCodeWhiteList> _ovoCodeWhiteList;

        private readonly IDictionary<string, string> _ovoCodeToNisCodes = new Dictionary<string, string>
        {
            { "OVO002000", "44000" },
            { "OVO002001", "44001" },
            { "OVO002002", "44002" },
        };

        public NisCodeAuthorizerTests()
        {
            _nisCodeService = new Mock<INisCodeService>();
            _nisCodeFinder = new Mock<INisCodeFinder<PersistentLocalId>>();
            _ovoCodeWhiteList = new Mock<IOvoCodeWhiteList>();
            _authorizer = new NisCodeAuthorizer<PersistentLocalId>(
                _nisCodeFinder.Object,
                _nisCodeService.Object,
                _ovoCodeWhiteList.Object);

            foreach (var (ovoCode, nisCode) in _ovoCodeToNisCodes)
            {
                _nisCodeService
                    .Setup(x => x.Get(ovoCode, CancellationToken.None))
                    .ReturnsAsync(nisCode);
            }
        }

        [Fact]
        public async Task WhenOvoCodeClaimIsMissing_ThenIsNotAuthorized()
        {
            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim(null) },
                new PersistentLocalId(1),
                CancellationToken.None);

            isNotAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNoNisCodeFoundForOvoCode_ThenIsNotAuthorized()
        {
            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim("OVO003000") },
                new PersistentLocalId(1),
                CancellationToken.None);

            isNotAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNoNisCodeFoundForRequest_ThenIsNotAuthorized()
        {
            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim(_ovoCodeToNisCodes.First().Key) },
                new PersistentLocalId(1),
                CancellationToken.None);

            isNotAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNisCodeFromOvoCodeDoesNotMatchNisCodeFoundForRequest_ThenIsNotAuthorized()
        {
            var ovoCodeFromClaim = _ovoCodeToNisCodes.First().Key;
            var nisCodeFromRequest = _ovoCodeToNisCodes.Skip(1).First().Value;

            var persistentLocalIdFromRequest = new PersistentLocalId(1);

            _nisCodeFinder
                .Setup(x => x.FindAsync(persistentLocalIdFromRequest, CancellationToken.None))
                .ReturnsAsync(nisCodeFromRequest);

            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim(ovoCodeFromClaim) },
                persistentLocalIdFromRequest,
                CancellationToken.None);

            isNotAuthorized.Should().BeTrue();
        }

        [Fact]
        public async Task WhenOvoCodeIsWhiteListed_ThenIsAuthorized()
        {
            var ovoCodeFromClaim = _ovoCodeToNisCodes.First().Key;

            _ovoCodeWhiteList
                .Setup(x => x.IsWhiteListed(ovoCodeFromClaim, CancellationToken.None))
                .ReturnsAsync(true);

            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim(ovoCodeFromClaim) },
                new PersistentLocalId(1),
                CancellationToken.None);

            isNotAuthorized.Should().BeFalse();
        }

        [Fact]
        public async Task WhenNisCodeFromOvoCodeMatchesNisCodeFoundForRequest_ThenIsAuthorized()
        {
            var ovoCodeFromClaim = _ovoCodeToNisCodes.First().Key;
            var nisCodeFromRequest = _ovoCodeToNisCodes.First().Value;

            var persistentLocalIdFromRequest = new PersistentLocalId(1);

            _nisCodeFinder
                .Setup(x => x.FindAsync(persistentLocalIdFromRequest, CancellationToken.None))
                .ReturnsAsync(nisCodeFromRequest);

            var isNotAuthorized = await _authorizer.IsNotAuthorized(
                new DefaultHttpContext { User = CreateUserWithOvoCodeClaim(ovoCodeFromClaim) },
                persistentLocalIdFromRequest,
                CancellationToken.None);

            isNotAuthorized.Should().BeFalse();
        }

        private ClaimsPrincipal CreateUserWithOvoCodeClaim(string? ovoCodeClaimValue)
        {
            if (string.IsNullOrWhiteSpace(ovoCodeClaimValue))
            {
                return new ClaimsPrincipal(new ClaimsIdentity(System.Array.Empty<Claim>(), "bearer"));
            }

            return new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(AcmIdmClaimTypes.VoOrgCode, ovoCodeClaimValue), }, "bearer"));
        }
    }
}
