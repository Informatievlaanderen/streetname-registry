namespace StreetNameRegistry.Tests.BackOffice.Api.WhenProposingStreetName
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Response;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenMunicipalityExistsNotSqs : BackOfficeApiTest<StreetNameController>
    {
        public GivenMunicipalityExistsNotSqs(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task ThenMediatorSends_StreetNameProposeRequest()
        {
            MockMediatorResponse<StreetNameProposeRequest, PersistentLocalIdETagResponse>(new PersistentLocalIdETagResponse(123, "hash"));

            await Controller.Propose(
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

            // Assert
            MockMediator.Verify(x => x.Send(It.IsAny<StreetNameProposeRequest>(), CancellationToken.None));
        }

        [Fact]
        public void WithOneOfStraatnamenAlreadyExists_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameProposeRequest>(), CancellationToken.None))
                .Throws(new StreetNameNameAlreadyExistsException("teststraat"));

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
                .Where(x => x.Message.Contains("Straatnaam 'teststraat' bestaat reeds in de gemeente."));
        }

        [Fact]
        public void WithMunicipalityRetired_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameProposeRequest>(), CancellationToken.None))
                .Throws(new MunicipalityHasInvalidStatusException(string.Empty));

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
                .Where(x => x.Message.Contains("Deze actie is enkel toegestaan binnen gemeenten met status 'voorgesteld' of 'inGebruik'."));
        }

        [Fact]
        public void WithNotSupportedLanguage_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameProposeRequest>(), CancellationToken.None))
                .Throws(new StreetNameNameLanguageIsNotSupportedException(string.Empty));

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
                .Where(x => x.Message.Contains("'Straatnamen' kunnen enkel voorkomen in de officiële of faciliteitentaal van de gemeente."));
        }

        [Fact]
        public void WithAMissingLanguage_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameProposeRequest>(), CancellationToken.None))
                .Throws(new StreetNameIsMissingALanguageException(string.Empty));

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
                .Where(x => x.Message.Contains("In 'Straatnamen' ontbreekt een officiële of faciliteitentaal."));
        }
    }
}
