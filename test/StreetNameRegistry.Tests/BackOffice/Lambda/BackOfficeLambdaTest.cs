namespace StreetNameRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Moq;
    using Municipality;
    using Municipality.Commands;
    using Newtonsoft.Json;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Response;
    using StreetNameRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using Testing;
    using TicketingService.Abstractions;
    using Xunit.Abstractions;

    public abstract class BackOfficeLambdaTest : StreetNameRegistryTest
    {
        protected BackOfficeLambdaTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>()
            where TException : Exception, new()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws<TException>();
            return idempotentCommandHandler;
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>(Func<TException> exceptionFactory)
            where TException : Exception
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws(exceptionFactory());
            return idempotentCommandHandler;
        }

        protected Mock<ITicketing> MockTicketing(Action<ETagResponse> ticketingCompleteCallback)
        {
            var ticketing = new Mock<ITicketing>();
            ticketing
                .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
                .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
                {
                    var eTagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticketResult.ResultAsJson!)!;
                    ticketingCompleteCallback(eTagResponse);
                });

            return ticketing;
        }

        protected void ProposeStreetName(
            MunicipalityId municipalityId,
            Names streetNameNames,
            PersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            var proposeCommand = new ProposeStreetName(
                municipalityId,
                streetNameNames,
                persistentLocalId,
                provenance);
            DispatchArrangeCommand(proposeCommand);
        }

        protected void RetireMunicipality(MunicipalityId municipalityId)
        {
            var retireMunicipality = new RetireMunicipality(
                municipalityId,
                Fixture.Create<RetirementDate>(),
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(retireMunicipality);
        }

        protected void AddOfficialLanguageFrench(MunicipalityId municipalityId)
        {
            var addOfficialLanguageFrench = new AddOfficialLanguageToMunicipality(
                municipalityId,
                Language.French,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(addOfficialLanguageFrench);
        }

        protected void AddOfficialLanguageDutch(MunicipalityId municipalityId)
        {
            var addOfficialLanguageDutch = new AddOfficialLanguageToMunicipality(
                municipalityId,
                Language.Dutch,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(addOfficialLanguageDutch);
        }

        protected void ImportMunicipality(MunicipalityId municipalityId, NisCode niscode)
        {
            var importMunicipality = new ImportMunicipality(
                municipalityId,
                niscode,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(importMunicipality);
        }

        protected void ApproveStreetName(MunicipalityId municipalityId, PersistentLocalId persistentLocalId)
        {
            var approveCommand = new ApproveStreetName(
                municipalityId,
                persistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(approveCommand);
        }

        protected void RejectStreetName(MunicipalityId municipalityId, PersistentLocalId persistentLocalId)
        {
            var rejectCommand = new RejectStreetName(
                municipalityId,
                persistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(rejectCommand);
        }

        protected void RetireStreetName(MunicipalityId municipalityId, PersistentLocalId persistentLocalId)
        {
            var retireStreetName = new RetireStreetName(
                municipalityId,
                persistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(retireStreetName);
        }

        protected void CorrectStreetNameName(
            MunicipalityId municipalityId,
            Names streetNameNames,
            PersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            var correctStreetNameNameCommand = new CorrectStreetNameNames(
                municipalityId,
                persistentLocalId,
                streetNameNames, provenance);
            DispatchArrangeCommand(correctStreetNameNameCommand);
        }

        protected void SetMunicipalityToCurrent(MunicipalityId municipalityId)
        {
            var setMunicipalityToCurrent = new SetMunicipalityToCurrent(
                municipalityId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(setMunicipalityToCurrent);
        }

        protected void AddFacilityLanguageToMunicipality(MunicipalityId municipalityId, Language language)
        {
            var addFacilityLanguageToMunicipality = new AddFacilityLanguageToMunicipality(
                municipalityId,
                language,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(addFacilityLanguageToMunicipality);
        }
    }
}
