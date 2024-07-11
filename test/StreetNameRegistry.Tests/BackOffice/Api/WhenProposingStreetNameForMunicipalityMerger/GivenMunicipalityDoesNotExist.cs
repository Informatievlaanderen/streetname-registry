namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Municipality;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityDoesNotExist : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenAggregateIdIsNotFound()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<ProposeStreetNamesForMunicipalityMergerSqsRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new AggregateIdIsNotFoundException());

            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator.Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(1));

            Func<Task> act = async () =>
            {
                await Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(CsvHelpers.Example),
                    "11001",
                    mockPersistentLocalIdGenerator.Object,
                CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "StraatnaamGemeenteNietGekendValidatie"
                        && e.ErrorMessage.Contains($"De gemeente '' is niet gekend in het gemeenteregister.")));
        }
    }
}
