namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenMunicipalityDoesNotExist : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper, useSqs: true)
        {
        }

        [Fact]
        public void ThenAggregateIdIsNotFound()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<SqsStreetNameProposeRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () => await Controller.Propose(
                ResponseOptions,
                MockPassingRequestValidator<StreetNameProposeRequest>(),
                new StreetNameProposeRequest
                {
                    GemeenteId = GetStreetNamePuri(123),
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                }, CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "code"
                    && e.ErrorMessage.Contains("message")));
        }
    }
}
