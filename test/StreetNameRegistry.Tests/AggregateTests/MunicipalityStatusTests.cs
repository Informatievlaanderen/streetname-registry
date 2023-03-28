namespace StreetNameRegistry.Tests.AggregateTests
{
    using System;
    using FluentAssertions;
    using Municipality;
    using Xunit;

    public class MunicipalityStatusTests
    {
        [Fact]
        public void GivenNonParseableString_ThenThrows()
        {
            Action act = () =>MunicipalityStatus.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }
}
