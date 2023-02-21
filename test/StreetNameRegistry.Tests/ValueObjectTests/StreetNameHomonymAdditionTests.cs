namespace StreetNameRegistry.Tests.ValueObjectTests;

using System;
using FluentAssertions;
using Municipality.Exceptions;
using Municipality;
using Xunit;

public class StreetNameHomonymAdditionTests
{
    [Fact]
    public void WhenValidHomonymAddition_ThenCreate()
    {
        var result = new StreetNameHomonymAddition("homonym is valid", Language.Dutch);
        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenHomonymAdditionCharacterLimitExceeded_ThenThrowHomonymAdditionMaxCharacterLengthExceededException()
    {
        var execute = () => new StreetNameHomonymAddition("homonym exceeding character limit", Language.Dutch);

        execute
            .Should()
            .Throw<HomonymAdditionMaxCharacterLengthExceededException>()
            .Where(item => item.LanguageCode == Language.Dutch.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WithNullOrEmptyValue_ThenThrowHomonymAdditionMaxCharacterLengthExceededException(string homonymAddition)
    {
        var execute = () => new StreetNameHomonymAddition(homonymAddition, Language.Dutch);

        execute
            .Should()
            .Throw<ArgumentNullException>()
            .Where(item => item.Message == "Value cannot be null or empty. (Parameter 'homonymAddition')");
    }
}
