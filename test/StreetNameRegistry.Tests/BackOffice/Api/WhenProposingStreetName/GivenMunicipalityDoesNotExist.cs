namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityDoesNotExist : BackOfficeApiTest<StreetNameController>
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

            var request = new StreetNameProposeRequest
            {
                GemeenteId = GetStreetNamePuri(123),
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "Rodekruisstraat" },
                    { Taal.FR, "Rue de la Croix-Rouge" }
                }
            };

            Func<Task> act = async () =>
            {
                await Controller.Propose(
                    ResponseOptions,
                    MockPassingRequestValidator<StreetNameProposeRequest>(),
                    request, CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "StraatnaamGemeenteNietGekendValidatie"
                        && e.ErrorMessage.Contains($"De gemeente '{request.GemeenteId}' is niet gekend in het gemeenteregister.")));
        }
    }
}
