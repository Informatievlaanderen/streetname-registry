namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenCorrectStreetNameHomonymAdditions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Municipality;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameHasInvalidStatus : BackOfficeLambdaTest
    {
        public GivenStreetNameHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new CorrectStreetNameHomonymAdditionsHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IMunicipalities>(),
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new CorrectStreetNameHomonymAdditionsLambdaRequest(Guid.NewGuid().ToString(),
                new CorrectStreetNameHomonymAdditionsSqsRequest
                {
                    PersistentLocalId = Fixture.Create<PersistentLocalId>(),
                    Request = new CorrectStreetNameHomonymAdditionsRequest { HomoniemToevoegingen = new Dictionary<Taal, string?>() },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.",
                        "StraatnaamGehistoreerdOfAfgekeurd"),
                    CancellationToken.None));
        }
    }
}
