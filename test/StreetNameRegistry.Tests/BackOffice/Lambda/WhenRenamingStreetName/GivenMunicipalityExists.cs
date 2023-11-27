namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenRenamingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
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

    public sealed class GivenMunicipalityExists : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customizations.Add(new WithUniqueInteger());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenStreetNameWasRenamed()
        {
            var municipalityId = Fixture.Create<MunicipalityId>();
            var sourceStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var destinationStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var nisCode = Fixture.Create<NisCode>();
            var sourceStreetNames = new Names(new Dictionary<Language, string> { { Language.Dutch, "Koning Leopold II laan" } });
            var destinationStreetNames = new Names(new Dictionary<Language, string> { { Language.Dutch, "Nieuwstraat" } });

            ImportMunicipality(municipalityId, nisCode);
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                sourceStreetNames,
                sourceStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());
            ApproveStreetName(municipalityId, sourceStreetNamePersistentLocalId);
            ProposeStreetName(
                municipalityId,
                destinationStreetNames,
                destinationStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var etag = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RenameStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { etag = result; }).Object,
                Container.Resolve<IMunicipalities>(),
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            //Act
            await handler.Handle(new RenameStreetNameLambdaRequest(municipalityId, new RenameStreetNameSqsRequest
            {
                Request = new RenameStreetNameRequest
                {
                    DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{destinationStreetNamePersistentLocalId}"
                },
                PersistentLocalId = sourceStreetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new MunicipalityStreamId(municipalityId)), 7, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.ETag);
        }

        [Fact]
        public async Task WhenSourceStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var sourceStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var destinationStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new RenameStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IMunicipalities>(),
                MockExceptionIdempotentCommandHandler(() => new StreetNameHasInvalidStatusException(sourceStreetNamePersistentLocalId)).Object);

            // Act
            await sut.Handle(new RenameStreetNameLambdaRequest(Guid.NewGuid().ToString(), new RenameStreetNameSqsRequest
            {
                Request = new RenameStreetNameRequest
                {
                    DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{destinationStreetNamePersistentLocalId}"
                },
                PersistentLocalId = sourceStreetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op straatnamen met status 'inGebruik'.",
                        "StraatnaamVoorgesteldAfgekeurdOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenDestinationStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var sourceStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var destinationStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new RenameStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IMunicipalities>(),
                MockExceptionIdempotentCommandHandler(() => new StreetNameHasInvalidStatusException(destinationStreetNamePersistentLocalId)).Object);

            // Act
            await sut.Handle(new RenameStreetNameLambdaRequest(Guid.NewGuid().ToString(), new RenameStreetNameSqsRequest
            {
                Request = new RenameStreetNameRequest
                {
                    DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{destinationStreetNamePersistentLocalId}"
                },
                PersistentLocalId = sourceStreetNamePersistentLocalId,
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
                        "StraatnaamAfgekeurdOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenMunicipalityHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var sourceStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var destinationStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var ticketing = new Mock<ITicketing>();

            var sut = new RenameStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IMunicipalities>(),
                MockExceptionIdempotentCommandHandler<MunicipalityHasInvalidStatusException>().Object);

            // Act
            await sut.Handle(new RenameStreetNameLambdaRequest(Guid.NewGuid().ToString(), new RenameStreetNameSqsRequest
            {
                Request = new RenameStreetNameRequest
                {
                    DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{destinationStreetNamePersistentLocalId}"
                },
                PersistentLocalId = sourceStreetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan binnen gemeenten met status 'inGebruik'.",
                        "StraatnaamGemeenteInGebruik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var municipalityId = Fixture.Create<MunicipalityId>();
            var sourceStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var destinationStreetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            var ticketing = new Mock<ITicketing>();

            ImportMunicipality(municipalityId, Fixture.Create<NisCode>());
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Koning Leopold II laan" } }),
                sourceStreetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var municipalities = Container.Resolve<IMunicipalities>();
            var sut = new RenameStreetNameHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                municipalities,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var municipality =
                await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            // Act
            await sut.Handle(new RenameStreetNameLambdaRequest(municipalityId, new RenameStreetNameSqsRequest
            {
                Request = new RenameStreetNameRequest
                {
                    DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{destinationStreetNamePersistentLocalId}"
                },
                PersistentLocalId = sourceStreetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, sourceStreetNamePersistentLocalId),
                            municipality.GetStreetNameHash(sourceStreetNamePersistentLocalId))),
                    CancellationToken.None));
        }
    }
}
