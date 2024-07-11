namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenProposingStreetNameForMunicipalityMerger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Municipality;
    using Municipality.Exceptions;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class SqsStreetNameProposeForMunicipalityMergerHandlerTests : BackOfficeLambdaTest
    {
        private readonly TestConsumerContext _consumerContext;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsStreetNameProposeForMunicipalityMergerHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenTheStreetNameIsProposed()
        {
            const int persistentLocalId = 5;

            //Arrange
            var municipalityLatestItem = _consumerContext.AddMunicipalityLatestItemFixtureWithNisCode("23002");

            var municipalityId = new MunicipalityId(municipalityLatestItem.MunicipalityId);
            var nisCode = "23002";

            ImportMunicipality(municipalityId, new NisCode(nisCode));
            AddOfficialLanguageDutch(municipalityId);

            var etag = new List<ETagResponse>();
            var ticketing = new Mock<ITicketing>();
            ticketing
                .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
                .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
                {
                    var eTagResponse = JsonConvert.DeserializeObject<List<ETagResponse>>(ticketResult.ResultAsJson!)!;
                    etag = eTagResponse;
                });

            var handler = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                Container.Resolve<IMunicipalities>());

            //Act
            await handler.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = municipalityLatestItem.NisCode,
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            persistentLocalId,
                            "Rodekruisstraat",
                            homonymAddition: "RR",
                            mergedStreetNamePersistentLocalIds: [persistentLocalId + 1]
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new MunicipalityStreamId(municipalityId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.First().ETag);
            stream.Messages.First().JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());

            var municipalityIdByPersistentLocalId = await _backOfficeContext.MunicipalityIdByPersistentLocalId.FindAsync(persistentLocalId);
            municipalityIdByPersistentLocalId.Should().NotBeNull();
            municipalityIdByPersistentLocalId.MunicipalityId.Should().Be(municipalityLatestItem.MunicipalityId);
            municipalityIdByPersistentLocalId.NisCode.Should().Be(nisCode);
        }

        [Fact]
        public async Task WhenStreetNameNameAlreadyExistsException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetname = "Bremt";

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new StreetNameNameAlreadyExistsException(streetname))
                    .Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        streetname,
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError($"Straatnaam '{streetname}' bestaat reeds in de gemeente.",
                        "StraatnaamBestaatReedsInGemeente"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenMunicipalityHasInvalidStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<MunicipalityHasInvalidStatusException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Deze actie is enkel toegestaan binnen gemeenten met status 'voorgesteld' of 'inGebruik'.", "StraatnaamGemeenteVoorgesteldOfInGebruik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameNameLanguageIsNotSupportedException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameNameLanguageIsNotSupportedException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(It.IsAny<Guid>(),
                    new TicketError(
                        "'Straatnamen' kunnen enkel voorkomen in de officiële of faciliteitentaal van de gemeente.",
                        "StraatnaamTaalNietInOfficieleOfFaciliteitenTaal"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameIsMissingALanguageException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameIsMissingALanguageException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "In 'Straatnamen' ontbreekt een officiële of faciliteitentaal.",
                        "StraatnaamOntbreektOfficieleOfFaciliteitenTaal"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenMergedStreetNamePersistentLocalIdsAreMissingException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<MergedStreetNamePersistentLocalIdsAreMissingException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: []
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "MergedStreetNamePersistentLocalIdsAreMissing",
                        "MergedStreetNamePersistentLocalIdsAreMissing"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenMergedStreetNamePersistentLocalIdsAreNotUniqueException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<MergedStreetNamePersistentLocalIdsAreNotUniqueException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        1,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: []
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "MergedStreetNamePersistentLocalIdsAreNotUnique",
                        "MergedStreetNamePersistentLocalIdsAreNotUnique"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var municipalityId = Fixture.Create<MunicipalityId>();
            var nisCode = Fixture.Create<NisCode>();
            var persistentLocalId = 123;

            ImportMunicipality(municipalityId, nisCode);
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);

            ProposeStreetName(
                municipalityId,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Bosstraat" } }),
                new PersistentLocalId(persistentLocalId),
                Fixture.Create<Provenance>());

            var ticketing = new Mock<ITicketing>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(456));
            var municipalities = Container.Resolve<IMunicipalities>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                _backOfficeContext,
                municipalities);

            // Act
            await sut.Handle(new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        persistentLocalId,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            }, CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new List<ETagResponse>
                    {
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, new PersistentLocalId(persistentLocalId)),
                            municipality.GetStreetNameHash(new PersistentLocalId(persistentLocalId)))
                    }),
                    CancellationToken.None));
        }

        [Fact]
        public async Task GivenRetryingRequest_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var municipalityId = Fixture.Create<MunicipalityId>();
            var nisCode = Fixture.Create<NisCode>();

            ImportMunicipality(municipalityId, nisCode);
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);

            var ticketing = new Mock<ITicketing>();
            var municipalities = Container.Resolve<IMunicipalities>();

            var proposeStreetNameHandler = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                municipalities);

            var persistentLocalId = 123;
            var request = new ProposeStreetNameForMunicipalityMergerLambdaRequest(municipalityId, new ProposeStreetNamesForMunicipalityMergerSqsRequest
            {
                NisCode = "11001",
                StreetNames =
                [
                    new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                    (
                        persistentLocalId,
                        "streetname",
                        homonymAddition: "RR",
                        mergedStreetNamePersistentLocalIds: [2]
                    )
                ],
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            })
            {
                MessageGroupId = municipalityId
            };

            // Act
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new List<ETagResponse>
                    {
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, new PersistentLocalId(persistentLocalId)),
                            municipality.GetStreetNameHash(new PersistentLocalId(persistentLocalId)))
                    }),
                    CancellationToken.None));
        }
    }
}
