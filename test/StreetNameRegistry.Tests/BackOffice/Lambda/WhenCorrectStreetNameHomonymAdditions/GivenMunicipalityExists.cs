namespace StreetNameRegistry.Tests.BackOffice.Lambda.WhenCorrectStreetNameHomonymAdditions
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
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Municipality;
    using Municipality.Commands;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using HomonymAdditions = StreetName.HomonymAdditions;
    using Language = StreetName.Language;
    using Names = StreetName.Names;
    using StreetNameHomonymAddition = StreetName.StreetNameHomonymAddition;
    using StreetNameId = StreetName.StreetNameId;
    using StreetNameStatus = StreetName.StreetNameStatus;

    public sealed class GivenMunicipalityExists : BackOfficeLambdaTest
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenStreetNameHomonymAdditionsWereCorrected()
        {
            // Arrange
            var municipalityId = Fixture.Create<MunicipalityId>();
            var streetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();
            var nisCode = "23002";

            ImportMunicipality(municipalityId, new NisCode(nisCode));
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);

            DispatchArrangeCommand(new MigrateStreetNameToMunicipality(
                new StreetName.MunicipalityId(municipalityId),
                Fixture.Create<StreetNameId>(),
                new StreetName.PersistentLocalId(streetNamePersistentLocalId),
                StreetNameStatus.Current,
                Language.Dutch,
                null,
                new Names(new Dictionary<Language, string> { { Language.Dutch, "Bremtstraat" } }),
                new HomonymAdditions(new[] { new StreetNameHomonymAddition("BO", Language.Dutch) }),
                true,
                false,
                Fixture.Create<Provenance>()));

            await _backOfficeContext.MunicipalityIdByPersistentLocalId.AddAsync(
                new MunicipalityIdByPersistentLocalId(streetNamePersistentLocalId, municipalityId, nisCode));
            await _backOfficeContext.SaveChangesAsync();

            var etag = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new CorrectStreetNameHomonymAdditionsHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(result => { etag = result; }).Object,
                Container.Resolve<IMunicipalities>(),
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            //Act
            await handler.Handle(new CorrectStreetNameHomonymAdditionsLambdaRequest(
                municipalityId,
                new CorrectStreetNameHomonymAdditionsSqsRequest
                {
                    PersistentLocalId = streetNamePersistentLocalId,
                    Request = new CorrectStreetNameHomonymAdditionsRequest { HomoniemToevoegingen = new Dictionary<Taal, string?> { { Taal.NL, "ZE" } }},
                    TicketId = Guid.NewGuid(),
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>()
                }), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new MunicipalityStreamId(municipalityId)), 4, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(etag.ETag);
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var municipalityId = Fixture.Create<MunicipalityId>();
            var streetNamePersistentLocalId = Fixture.Create<PersistentLocalId>();

            ImportMunicipality(municipalityId, Fixture.Create<NisCode>());
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                new StreetNameRegistry.Municipality.Names(new Dictionary<StreetNameRegistry.Municipality.Language, string>
                {
                    { StreetNameRegistry.Municipality.Language.Dutch, "Bremtstraat" }
                }),
                streetNamePersistentLocalId,
                Fixture.Create<Provenance>());

            var municipalities = Container.Resolve<IMunicipalities>();

            var handler = new CorrectStreetNameHomonymAdditionsHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                municipalities,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            var municipality = await municipalities.GetAsync(new MunicipalityStreamId(municipalityId), CancellationToken.None);

            // Act
            await handler.Handle(new CorrectStreetNameHomonymAdditionsLambdaRequest(
                municipalityId,
                new CorrectStreetNameHomonymAdditionsSqsRequest
                {
                    PersistentLocalId = streetNamePersistentLocalId,
                    Request = new CorrectStreetNameHomonymAdditionsRequest { HomoniemToevoegingen = new Dictionary<Taal, string?> { { Taal.NL, "ZE" } }},
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
                            string.Format(ConfigDetailUrl, streetNamePersistentLocalId),
                            municipality.GetStreetNameHash(streetNamePersistentLocalId))),
                    CancellationToken.None));
        }
    }
}
