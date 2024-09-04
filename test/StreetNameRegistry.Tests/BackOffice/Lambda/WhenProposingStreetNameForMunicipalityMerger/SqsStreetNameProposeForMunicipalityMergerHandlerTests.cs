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
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Municipality;
    using Municipality.Exceptions;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class SqsStreetNameProposeForMunicipalityMergerHandlerTests : BackOfficeLambdaTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsStreetNameProposeForMunicipalityMergerHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext([]);
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext([]);
        }

        [Fact]
        public async Task ThenTheStreetNameIsProposed()
        {
            //Arrange
            var oldStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var newStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var newMunicipalityId = Fixture.Create<MunicipalityId>();
            var oldMunicipalityId = Fixture.Create<MunicipalityId>();
            const string newNisCode = "23002";

            ImportMunicipality(oldMunicipalityId, new NisCode("23001"));
            ProposeStreetName(oldMunicipalityId, Fixture.Create<Names>(), oldStreetNamePersistentLocalId, Fixture.Create<Provenance>());

            ImportMunicipality(newMunicipalityId, new NisCode(newNisCode));
            AddOfficialLanguageDutch(newMunicipalityId);

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
                new FakeScopedIdemponentCommandHandler(() => new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext)),
                _backOfficeContext,
                Container.Resolve<IMunicipalities>(),
                new NullLoggerFactory());

            //Act
            await handler.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = newNisCode,
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            newStreetNamePersistentLocalId,
                            "Rodekruisstraat",
                            homonymAddition: "RR",
                            mergedStreetNames: [new MergedStreetName(oldStreetNamePersistentLocalId, oldMunicipalityId)]
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
            }, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new MunicipalityStreamId(newMunicipalityId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.First().ETag);
            stream.Messages.First().JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());

            var municipalityIdByPersistentLocalId =
                await _backOfficeContext.MunicipalityIdByPersistentLocalId.FindAsync((int)newStreetNamePersistentLocalId);
            municipalityIdByPersistentLocalId.Should().NotBeNull();
            municipalityIdByPersistentLocalId!.MunicipalityId.Should().Be((Guid)newMunicipalityId);
            municipalityIdByPersistentLocalId.NisCode.Should().Be(newNisCode);
        }

        [Fact]
        public async Task WhenStreetNameNameAlreadyExistsException_ThenTicketingErrorIsExpected()
        {
            //Arrange
            var newMunicipalityId = Fixture.Create<MunicipalityId>();
            const string streetname = "Bremt";

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler(() => new StreetNameNameAlreadyExistsException(streetname)).Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "23002",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            streetname,
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
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
            var newMunicipalityId = Fixture.Create<MunicipalityId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<MunicipalityHasInvalidStatusException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "23002",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Deze actie is enkel toegestaan binnen gemeenten met status 'voorgesteld' of 'inGebruik'.",
                        "StraatnaamGemeenteVoorgesteldOfInGebruik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenStreetNameNameLanguageIsNotSupportedException_ThenTicketingErrorIsExpected()
        {
            const string newNisCode = "23002";

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<StreetNameNameLanguageIsNotSupportedException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            // Act
            var municipalityId = Guid.NewGuid().ToString();
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(municipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = newNisCode,
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
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
            //Arrange
            var newMunicipalityId = Fixture.Create<MunicipalityId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<StreetNameIsMissingALanguageException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            // Act
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "23002",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
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
            var newMunicipalityId = Fixture.Create<MunicipalityId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<MergedStreetNamePersistentLocalIdsAreMissingException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            // Act
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "11001",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
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
            var newMunicipalityId = Fixture.Create<MunicipalityId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<MergedStreetNamePersistentLocalIdsAreNotUniqueException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            // Act
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "11001",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
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
        public async Task WhenStreetNameHasInvalidDesiredStatusException_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var newMunicipalityId = Fixture.Create<MunicipalityId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionScopedIdempotentCommandHandler<StreetNameHasInvalidDesiredStatusException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>(),
                new NullLoggerFactory());

            // Act
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "11001",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            Fixture.Create<PersistentLocalId>(),
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: []
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Desired status should be proposed or current",
                        "StreetNameHasInvalidDesiredStatus"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            //Arrange
            var oldStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var newStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var newMunicipalityId = Fixture.Create<MunicipalityId>();
            var oldMunicipalityId = Fixture.Create<MunicipalityId>();
            const string newNisCode = "23002";

            ImportMunicipality(oldMunicipalityId, new NisCode("23001"));
            ProposeStreetName(oldMunicipalityId, Fixture.Create<Names>(), oldStreetNamePersistentLocalId, Fixture.Create<Provenance>());

            ImportMunicipality(newMunicipalityId, new NisCode(newNisCode));
            AddOfficialLanguageDutch(newMunicipalityId);
            ProposeStreetName(
                newMunicipalityId,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Bosstraat" } }),
                new PersistentLocalId(newStreetNamePersistentLocalId),
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
                MockExceptionScopedIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                _backOfficeContext,
                municipalities,
                new NullLoggerFactory());

            // Act
            await sut.Handle(new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "11001",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            newStreetNamePersistentLocalId,
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: [new MergedStreetName(oldStreetNamePersistentLocalId, oldMunicipalityId)]
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
            }, CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(newMunicipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new List<ETagResponse>
                    {
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, newStreetNamePersistentLocalId),
                            municipality.GetStreetNameHash(newStreetNamePersistentLocalId))
                    }),
                    CancellationToken.None));
        }

        [Fact]
        public async Task GivenRetryingRequest_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var oldStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var newStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var newMunicipalityId = Fixture.Create<MunicipalityId>();
            var oldMunicipalityId = Fixture.Create<MunicipalityId>();
            const string newNisCode = "23002";

            ImportMunicipality(oldMunicipalityId, new NisCode("23001"));
            ProposeStreetName(oldMunicipalityId, Fixture.Create<Names>(), oldStreetNamePersistentLocalId, Fixture.Create<Provenance>());

            ImportMunicipality(newMunicipalityId, new NisCode(newNisCode));
            AddOfficialLanguageDutch(newMunicipalityId);

            var ticketing = new Mock<ITicketing>();

            var municipalities = Container.Resolve<IMunicipalities>();
            var proposeStreetNameHandler = new ProposeStreetNameForMunicipalityMergerHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new FakeScopedIdemponentCommandHandler(() => new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext)),
                _backOfficeContext,
                municipalities,
                new NullLoggerFactory());

            var request = new ProposeStreetNamesForMunicipalityMergerLambdaRequest(newMunicipalityId,
                new ProposeStreetNamesForMunicipalityMergerSqsRequest
                {
                    NisCode = "11001",
                    StreetNames =
                    [
                        new ProposeStreetNamesForMunicipalityMergerSqsRequestItem
                        (
                            newStreetNamePersistentLocalId,
                            "streetname",
                            homonymAddition: "RR",
                            mergedStreetNames: [new MergedStreetName(oldStreetNamePersistentLocalId, oldMunicipalityId)]
                        )
                    ],
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                })
            {
                MessageGroupId = newMunicipalityId
            };

            // Act
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(newMunicipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new List<ETagResponse>
                    {
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, newStreetNamePersistentLocalId),
                            municipality.GetStreetNameHash(newStreetNamePersistentLocalId))
                    }),
                    CancellationToken.None));
        }

        private class FakeScopedIdemponentCommandHandler : IScopedIdempotentCommandHandler
        {
            private readonly Func<IIdempotentCommandHandler> _idempotentCommandHandlerResolver;

            public FakeScopedIdemponentCommandHandler(Func<IIdempotentCommandHandler> idempotentCommandHandlerResolver)
            {
                _idempotentCommandHandlerResolver = idempotentCommandHandlerResolver;
            }

            public Task<long> Dispatch(Guid? commandId, object command, IDictionary<string, object> metadata, CancellationToken cancellationToken)
            {
                var handler = _idempotentCommandHandlerResolver();
                return handler.Dispatch(commandId, command, metadata, cancellationToken);
            }
        }
    }
}
