namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Linq;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Consumer.Municipality;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Municipality;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeApiTest<StreetNameController>
    {
        private readonly TestConsumerContext _municipalityConsumerContext;

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _municipalityConsumerContext = new FakeConsumerContextFactory().CreateDbContext();
        }

        [Fact]
        public void WithNoFormFile_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    null,
                    "bla",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Please upload a CSV file.");
        }

        [Fact]
        public void WithNoCsvExtension_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("file", "content"),
                    "bla",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Only CSV files are allowed.");
        }

        [Fact]
        public void WithNoNisCode_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("OUD NIS code;OUD straatnaamid;NIEUW NIS code;NIEUW straatnaam;NIEUW homoniemtoevoeging\n" +
                                                        "11001;123;;Name;HO"),
                    "bla",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("NisCode is required at record number 1");
        }

        [Fact]
        public void WithDifferentNisCodeThanRoute_ThenReturnsBadRequest()
        {
            var nisCode = "bla";
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("OUD NIS code;OUD straatnaamid;NIEUW NIS code;NIEUW straatnaam;NIEUW homoniemtoevoeging\n" +
                                                        "11000;123;11001;Name;HO"),
                    nisCode,
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo($"NisCode 11001 does not match the provided NisCode {nisCode} at record number 1");
        }

        [Fact]
        public void WithNoStreetName_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("OUD NIS code;OUD straatnaamid;NIEUW NIS code;NIEUW straatnaam;NIEUW homoniemtoevoeging\n" +
                                                        "11000;123;NisCode;;HO"),
                    "NisCode",
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("StreetName is required at record number 1");
        }

        [Fact]
        public void WithValidCsv_ThenReturnsOk()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));
            MockMediatorResponse<ProposeStreetNamesForMunicipalityMergerSqsRequest, LocationResult>(expectedLocationResult);

            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator.Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(1));

            const string oldNisCode = "11000";
            var oldMunicipalityId = Guid.NewGuid();
            _municipalityConsumerContext.Add(new MunicipalityConsumerItem
            {
                MunicipalityId = oldMunicipalityId,
                NisCode = oldNisCode
            });
            _municipalityConsumerContext.SaveChanges();

            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString($"OUD NIS code;OUD straatnaamid;NIEUW NIS code;NIEUW straatnaam;NIEUW homoniemtoevoeging\n" +
                                                        $"{oldNisCode};123;11001;Street;HO\n{oldNisCode};456;11001;Name;NYM\n{oldNisCode};789;11001;Street;HO"),
                    "11001",
                    mockPersistentLocalIdGenerator.Object,
                    _municipalityConsumerContext,
                    CancellationToken.None).GetAwaiter().GetResult();

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeStreetNamesForMunicipalityMergerSqsRequest>(request =>
                        request.NisCode == "11001" &&
                        request.StreetNames.Count == 2 &&
                        request.StreetNames.TrueForAll(y => y.MergedStreetNames.All(z => z.MunicipalityId == oldMunicipalityId)) &&
                        request.ProvenanceData.Timestamp != Instant.MinValue &&
                        request.ProvenanceData.Application == Application.StreetNameRegistry &&
                        request.ProvenanceData.Modification == Modification.Insert),
                    It.IsAny<CancellationToken>()), Times.Once);

            result.Should().BeOfType<AcceptedResult>();

            var acceptedResult = (AcceptedResult)result;
            acceptedResult.Location.Should().NotBeNull();
            AssertLocation(acceptedResult.Location, ticketId);
        }
    }
}
