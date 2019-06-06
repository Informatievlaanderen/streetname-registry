namespace StreetNameRegistry.Tests
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using FluentAssertions;
    using StreetName.Commands;
    using Xunit;

    public class When_creating_deterministic_CommandId
    {
        [Fact]
        public void Given_ImportStreetNameFromCrabCommand_Then_CommandId_is_expected()
        {
            var expectedGuid = new Guid("2a9d9a37-2588-5952-aa20-ff005e3997ab");
            var importStreetNameFromCrabCommand = new ImportStreetNameFromCrab(
                new CrabStreetNameId(1),
                new CrabMunicipalityId(1),
                new NisCode("11001"),
                new CrabStreetName("Acacialaan", CrabLanguage.Dutch),
                new CrabStreetName(null, CrabLanguage.English),
                new CrabTransStreetName("ACACIAL", CrabLanguage.Dutch),
                new CrabTransStreetName(null, CrabLanguage.English),
                CrabLanguage.Dutch,
                CrabLanguage.English,
                new CrabLifetime(new DateTime(1830, 1, 1).ToCrabLocalDateTime(), null),
                new CrabTimestamp(new DateTime(2013, 4, 12, 20, 06, 58).ToCrabInstant()),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                CrabOrganisation.Municipality);

            var @string = importStreetNameFromCrabCommand.ToString();
            @string.Should().Be(@"1, 1, 11001, Acacialaan (Dutch),  (English), ACACIAL (Dutch),  (English), Dutch, English, 1830-01-01T00:00:00.000000000, 2013-04-12T18:06:58Z, VLM\CRABSSISservice, Correction, Municipality");

            var createdId = importStreetNameFromCrabCommand.CreateCommandId();
            createdId.Should().Be(expectedGuid);
        }

        [Fact]
        public void Given_ImportStreetNameStatusFromCrabCommand_Then_CommandId_is_expected()
        {
            var expectedGuid = new Guid("f0d2c06f-1ac9-517d-9366-5b107ed4410c");
            var importStreetNameStatusFromCrabCommand = new ImportStreetNameStatusFromCrab(
                new CrabStreetNameStatusId(1),
                new CrabStreetNameId(1),
                CrabStreetNameStatus.InUse,
                new CrabLifetime(new DateTime(1830, 1, 1).ToCrabLocalDateTime(), null),
                new CrabTimestamp(new DateTime(2013, 4, 12, 20, 07, 26).ToCrabInstant()),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                CrabOrganisation.Municipality);

            var @string = importStreetNameStatusFromCrabCommand.ToString();
            @string.Should().Be(@"1, 1, InUse, 1830-01-01T00:00:00.000000000, 2013-04-12T18:07:26Z, VLM\CRABSSISservice, Correction, Municipality");

            var createdId = importStreetNameStatusFromCrabCommand.CreateCommandId();
            createdId.Should().Be(expectedGuid);
        }
    }
}
