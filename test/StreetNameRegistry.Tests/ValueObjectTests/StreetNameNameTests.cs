namespace StreetNameRegistry.Tests.ValueObjectTests
{
    using FluentAssertions;
    using Municipality;
    using Xunit;

    public class StreetNameNameTests
    {
        [Theory]
        [InlineData("Bremstraat", "Bremstraat")]
        [InlineData("bremstraat", "BREMSTRAAT")]
        public void Equality(string a, string b)
        {
            new StreetNameName(a, Language.Dutch).Should().Be(new StreetNameName(b, Language.Dutch));
        }

        [Theory]
        [InlineData("BREMSTRAAT")]
        [InlineData("bremstraat")]
        public void ToStringShouldKeepCasing(string name)
        {
            new StreetNameName(name, Language.Dutch)
                .ToString().Should().Be($"{name} (Dutch)");
        }
    }
}
