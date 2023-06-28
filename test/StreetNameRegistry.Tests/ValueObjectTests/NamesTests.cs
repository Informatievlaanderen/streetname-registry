namespace StreetNameRegistry.Tests.ValueObjectTests
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Municipality;
    using Xunit;

    public class NamesTests
    {
        [Fact]
        public void GivenDictionary_FilterOutNullNames()
        {
            var sut = new Names(new Dictionary<Language, string>
            {
                {Language.Dutch, "Landgraaf"},
                {Language.French, ""},
                {Language.German, " "},
                {Language.English, null},
            });

            sut.Should().HaveCount(1);
        }

        [Fact]
        public void GivenIEnumerable_FilterOutNullNames()
        {
            var sut = new Names(new List<StreetNameName>
            {
                new StreetNameName("Landgraaf", Language.Dutch),
                new StreetNameName("", Language.French),
                new StreetNameName(" ", Language.German),
                new StreetNameName(null, Language.English),
            });

            sut.Should().HaveCount(1);
        }
    }
}
