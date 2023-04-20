﻿namespace StreetNameRegistry.Tests.BackOffice.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization;
    using Xunit;

    public class MunicipalityNisCodeFinderTests
    {
        private readonly MunicipalityRegistryNisCodeFinder _registryNisCodeFinder = new MunicipalityRegistryNisCodeFinder();

        [Fact]
        public async Task GivenValidMunicipalityPuri()
        {
            const string nisCode = "55001";
            const string puri = $"https://data.vlaanderen.be/id/gemeente/{nisCode}";

            var result = await _registryNisCodeFinder.FindAsync(new MunicipalityPuri(puri), CancellationToken.None);

            result.Should().Be(nisCode);
        }

        [Fact]
        public async Task GivenInvalidMunicipalityPuri()
        {
            var result = await _registryNisCodeFinder.FindAsync(new MunicipalityPuri("55001"), CancellationToken.None);

            result.Should().BeNullOrEmpty();
        }
    }
}
