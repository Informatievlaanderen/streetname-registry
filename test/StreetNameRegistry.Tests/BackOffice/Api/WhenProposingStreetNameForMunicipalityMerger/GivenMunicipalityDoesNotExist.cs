namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Linq;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Consumer.Municipality;
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
        private readonly TestConsumerContext _municipalityConsumerContext;

        public GivenMunicipalityDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _municipalityConsumerContext = new FakeConsumerContextFactory().CreateDbContext();
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

            _municipalityConsumerContext.Add(new MunicipalityConsumerItem
            {
                MunicipalityId = Guid.NewGuid(),
                NisCode = CsvHelpers.OldNisCode
            });
            _municipalityConsumerContext.SaveChanges();

            var act = async () =>
            {
                await Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString(CsvHelpers.Example),
                    "11001",
                    mockPersistentLocalIdGenerator.Object,
                    _municipalityConsumerContext);
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
