namespace StreetNameRegistry.Tests.BackOffice.Api.WhenCorrectingStreetNameName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using Municipality.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using MunicipalityId = Municipality.MunicipalityId;
    using PersistentLocalId = Municipality.PersistentLocalId;

    public class GivenMunicipalityExistsNotSqs : BackOfficeApiTest<StreetNameController>
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenMunicipalityExistsNotSqs(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenMediatorSends_StreetNameCorrectNamesRequest()
        {
            var municipalityId = new MunicipalityId(Guid.NewGuid());
            var persistentLocalId = new PersistentLocalId(456);

            _backOfficeContext.AddMunicipalityIdByPersistentLocalIdToFixture(persistentLocalId, municipalityId);

            MockMediatorResponse<StreetNameCorrectNamesRequest, ETagResponse>(new ETagResponse("location", "hash"));

            // Act
            var result = (AcceptedWithETagResult)await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                persistentLocalId,
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                ifMatchHeaderValue: null,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None));
            result.ETag.Should().Be("hash");
        }

        [Fact]
        public void WhenStreetNameIsNotFound_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None))
                .Throws(new StreetNameIsNotFoundException());

            //Act
            Func<Task> act = async () => await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                new PersistentLocalId(456),
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x => x.Message.Contains("Onbestaande straatnaam")
                            && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenStreetNameIsRemoved_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None))
                .Throws(new StreetNameIsRemovedException());

            //Act
            Func<Task> act = async () => await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                new PersistentLocalId(456),
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x => x.Message.Contains("Verwijderde straatnaam")
                            && x.StatusCode == StatusCodes.Status410Gone);
        }

        [Fact]
        public void WhenStreetNameIsNotInStatusProposedOrCurrent_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None))
                .Throws(new StreetNameHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                new PersistentLocalId(456),
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error =>
                        error.ErrorCode == "StraatnaamGehistoreerdOfAfgekeurd"
                        && error.ErrorMessage.Equals("Deze actie is enkel toegestaan op straatnamen met status 'voorgesteld' of 'inGebruik'.")));
        }

        [Fact]
        public void WithOneOfStraatnamenAlreadyExists_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None))
                .Throws(new StreetNameNameAlreadyExistsException("teststraat"));

            //Act
            Func<Task> act = async () => await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                new PersistentLocalId(456),
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Message.Contains("Straatnaam 'teststraat' bestaat reeds in de gemeente."));
        }

        [Fact]
        public void WithNotSupportedLanguage_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectNamesRequest>(), CancellationToken.None))
                .Throws(new StreetNameNameLanguageIsNotSupportedException(string.Empty));

            //Act
            Func<Task> act = async () => await Controller.CorrectStreetNameNames(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectNamesRequest>(),
                ResponseOptions,
                new PersistentLocalId(456),
                new StreetNameCorrectNamesRequest
                {
                    Straatnamen = new Dictionary<Taal, string>
                    {
                        {Taal.NL, "Rodekruisstraat"},
                        {Taal.FR, "Rue de la Croix-Rouge"}
                    }
                },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Message.Contains("'Straatnamen' kunnen enkel voorkomen in de officiële of faciliteitentaal van de gemeente."));
        }
    }
}
