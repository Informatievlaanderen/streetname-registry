namespace StreetNameRegistry.Tests.BackOffice.Handlers.WhenCorrectingStreetNameApproval
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
    using FluentAssertions;
    using global::AutoFixture;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers;
    using Municipality;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExists : BackOfficeHandlerTest
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenMunicipalityExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenStreetNameApprovalWasCorrected()
        {
            var municipalityId = new MunicipalityId(Guid.NewGuid());
            var streetNamePersistentLocalId = new PersistentLocalId(456);
            var provenance = Fixture.Create<Provenance>();

            ImportMunicipality(municipalityId, new NisCode("23002"));
            SetMunicipalityToCurrent(municipalityId);
            AddOfficialLanguageDutch(municipalityId);
            ProposeStreetName(
                municipalityId,
                new Names(new Dictionary<Language, string>{{Language.Dutch, "Bremt"}}),
                streetNamePersistentLocalId,
                provenance);
            ApproveStreetName(
                municipalityId,
                streetNamePersistentLocalId);

            await _backOfficeContext.MunicipalityIdByPersistentLocalId.AddAsync(
                new MunicipalityIdByPersistentLocalId(streetNamePersistentLocalId, municipalityId));
            await _backOfficeContext.SaveChangesAsync();

            var handler = new StreetNameCorrectApprovalHandler(
                Container.Resolve<ICommandHandlerResolver>(),
                _backOfficeContext,
                Container.Resolve<IMunicipalities>(),
                _idempotencyContext);

            //Act
            var response = await handler.Handle(new StreetNameCorrectApprovalRequest
            {
                PersistentLocalId = streetNamePersistentLocalId
            }, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new MunicipalityStreamId(municipalityId)), 5, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(response.ETag);
        }
    }
}
