namespace StreetNameRegistry.Tests.AggregateTests
{
    using FluentAssertions;
    using Municipality;
    using Xunit;

    public class LevenshteinDistanceCalculatorTests
    {
        [Theory]
        [InlineData("0123456789", "012345678-", 10)]
        [InlineData("0123456789", "01234567--", 20)]
        [InlineData("0123456789", "0123456---", 30)]
        [InlineData("0123456789", "56789", 50)]
        [InlineData("0123456789", "9876543210", 100)]
        public void HappyScenario(string s1, string s2, int changeDifference)
        {
            // Act
            var difference = LevenshteinDistanceCalculator.CalculatePercentage(s1, s2);

            // Assert
            difference.Should().Be(changeDifference);
        }
    }
}
