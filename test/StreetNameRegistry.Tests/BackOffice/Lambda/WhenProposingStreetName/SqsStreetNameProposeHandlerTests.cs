namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Municipality;
    using Municipality.Exceptions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class SqsStreetNameProposeHandlerTests : BackOfficeLambdaTest
    {
        private readonly TestConsumerContext _consumerContext;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsStreetNameProposeHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
            AddOfficialLanguageFrench(municipalityId);

            var etag = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { etag = result; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                Container.Resolve<IMunicipalities>());

            //Act
            await handler.Handle(new ProposeStreetNameLambdaRequest(municipalityId, new ProposeStreetNameSqsRequest
                {
                    PersistentLocalId = new PersistentLocalId(persistentLocalId),
                    Request = new ProposeStreetNameRequest
                    {
                        GemeenteId = $"https://data.vlaanderen.be/id/gemeente/{municipalityLatestItem.NisCode}",
                        Straatnamen = new Dictionary<Taal, string>
                        {
                            { Taal.NL, "Rodekruisstraat" },
                            { Taal.FR, "Rue de la Croix-Rouge" }
                        }
                    },
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new MunicipalityStreamId(municipalityId)), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.ETag);
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

            var sut = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new StreetNameNameAlreadyExistsException(streetname))
                    .Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            await sut.Handle(new ProposeStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ProposeStreetNameSqsRequest
            {
                Request = new ProposeStreetNameRequest { Straatnamen = new Dictionary<Taal, string>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

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

            var sut = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<MunicipalityHasInvalidStatusException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            await sut.Handle(new ProposeStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ProposeStreetNameSqsRequest
            {
                Request = new ProposeStreetNameRequest { Straatnamen = new Dictionary<Taal, string>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

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

            var sut = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameNameLanguageIsNotSupportedException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            await sut.Handle(new ProposeStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ProposeStreetNameSqsRequest
            {
                Request = new ProposeStreetNameRequest { Straatnamen = new Dictionary<Taal, string>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

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

            var sut = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<StreetNameIsMissingALanguageException>().Object,
                _backOfficeContext,
                Mock.Of<IMunicipalities>());

            // Act
            await sut.Handle(new ProposeStreetNameLambdaRequest(Guid.NewGuid().ToString(), new ProposeStreetNameSqsRequest
            {
                Request = new ProposeStreetNameRequest { Straatnamen = new Dictionary<Taal, string>() },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

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
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var municipalityId = Fixture.Create<MunicipalityId>();
            var nisCode = Fixture.Create<NisCode>();

            ImportMunicipality(municipalityId, nisCode);
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Bosstraat" } }),
                new PersistentLocalId(123),
                Fixture.Create<Provenance>());

            var ticketing = new Mock<ITicketing>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(new PersistentLocalId(456));
            var municipalities = Container.Resolve<IMunicipalities>();

            var proposeStreetNameHandler = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                _backOfficeContext,
                municipalities);

            // Act
            await proposeStreetNameHandler.Handle(new ProposeStreetNameLambdaRequest(municipalityId.ToString(), new ProposeStreetNameSqsRequest
            {
                PersistentLocalId = new PersistentLocalId(123),
                Request = new ProposeStreetNameRequest { GemeenteId = "http://puri/11001", Straatnamen = new Dictionary<Taal, string> { { Taal.NL, "Bosstraat" } }},
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new ETagResponse(
                        string.Format(ConfigDetailUrl, new PersistentLocalId(123)),
                        municipality.GetStreetNameHash(new PersistentLocalId(123)))),
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

            var proposeStreetNameHandler = new ProposeStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                _backOfficeContext,
                municipalities);

            var request = new ProposeStreetNameLambdaRequest(municipalityId.ToString(), new ProposeStreetNameSqsRequest
            {
                PersistentLocalId = new PersistentLocalId(123),
                Request = new ProposeStreetNameRequest { GemeenteId = "http://puri/11001", Straatnamen = new Dictionary<Taal, string> { { Taal.NL, "Bosstraat" } } },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            // Act
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);
            await proposeStreetNameHandler.Handle(request, CancellationToken.None);

            //Assert
            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(new ETagResponse(
                        string.Format(ConfigDetailUrl, new PersistentLocalId(123)),
                        municipality.GetStreetNameHash(new PersistentLocalId(123)))),
                    CancellationToken.None));
        }
    }
}
