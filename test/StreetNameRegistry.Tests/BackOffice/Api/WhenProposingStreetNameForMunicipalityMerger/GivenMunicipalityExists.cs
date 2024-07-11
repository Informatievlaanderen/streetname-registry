namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
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
        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void WithNoFormFile_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    null,
                    "bla",
                    Mock.Of<IPersistentLocalIdGenerator>(),
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
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo("Only CSV files are allowed.");
        }

        [Fact]
        public void WithNoNisCode_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("http://b/123;;Name;HO"),
                    "bla",
                    Mock.Of<IPersistentLocalIdGenerator>(),
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
                    CsvHelpers.CreateFormFileFromString("http://b/123;11001;Name;HO"),
                    nisCode,
                    Mock.Of<IPersistentLocalIdGenerator>(),
                    CancellationToken.None).GetAwaiter().GetResult();

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().BeEquivalentTo($"NisCode 11001 does not match the provided NisCode {nisCode} at record number 1");
        }

        [Fact]
        public void WithNoStreetName_ThenReturnsBadRequest()
        {
            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("http://b/123;NisCode;;HO"),
                    "NisCode",
                    Mock.Of<IPersistentLocalIdGenerator>(),
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

            var result =
                Controller.ProposeForMunicipalityMerger(
                    CsvHelpers.CreateFormFileFromString("http://a/123;11001;Street;HO\nhttp://a/456;11001;Name;NYM\nhttp://a/789;11001;Street;HO"),
                    "11001",
                    mockPersistentLocalIdGenerator.Object,
                    CancellationToken.None).GetAwaiter().GetResult();

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ProposeStreetNamesForMunicipalityMergerSqsRequest>(request =>
                        request.NisCode == "11001" &&
                        request.StreetNames.Count == 2 &&
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
