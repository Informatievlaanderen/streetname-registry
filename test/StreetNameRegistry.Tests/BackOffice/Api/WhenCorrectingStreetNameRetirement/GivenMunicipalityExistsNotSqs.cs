namespace StreetNameRegistry.Tests.BackOffice.Api.WhenCorrectingStreetNameRetirement
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using Municipality.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using MunicipalityId = Municipality.MunicipalityId;
    using PersistentLocalId = Municipality.PersistentLocalId;

    public sealed class GivenMunicipalityExistsNotSqs : BackOfficeApiTest<StreetNameController>
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenMunicipalityExistsNotSqs(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenMediatorSends_StreetNameCorrectRetirementRequest()
        {
            var municipalityId = Fixture.Create<MunicipalityId>();
            var persistentLocalId = Fixture.Create<PersistentLocalId>();

            _backOfficeContext.AddMunicipalityIdByPersistentLocalIdToFixture(persistentLocalId, municipalityId);

            MockMediatorResponse<StreetNameCorrectRetirementRequest, ETagResponse>(new ETagResponse("location", "hash"));

            // Act
            var result = (NoContentWithETagResult)await Controller.CorrectRetirement(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectRetirementRequest>(),
                new StreetNameCorrectRetirementRequest { PersistentLocalId = persistentLocalId },
                ifMatchHeaderValue: null,
                CancellationToken.None);

            // Assert
            MockMediator.Verify(x => x.Send(It.IsAny<StreetNameCorrectRetirementRequest>(), CancellationToken.None));
            result.ETag.Should().Be("hash");
        }

        [Fact]
        public void WhenStreetNameIsNotFound_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectRetirementRequest>(), CancellationToken.None))
                .Throws(new StreetNameIsNotFoundException());

            //Act
            Func<Task> act = async () => await Controller.CorrectRetirement(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectRetirementRequest>(),
                new StreetNameCorrectRetirementRequest { PersistentLocalId = Fixture.Create<PersistentLocalId>() },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande straatnaam")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenStreetNameIsRemoved_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectRetirementRequest>(), CancellationToken.None))
                .Throws(new StreetNameIsRemovedException());

            //Act
            Func<Task> act = async () => await Controller.CorrectRetirement(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectRetirementRequest>(),
                new StreetNameCorrectRetirementRequest { PersistentLocalId = Fixture.Create<PersistentLocalId>() },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Verwijderde straatnaam")
                    && x.StatusCode == StatusCodes.Status410Gone);
        }

        [Fact]
        public void WhenStreetNameIsNotInStatusCurrent_ThenBadRequestIsExpected()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectRetirementRequest>(), CancellationToken.None))
                .Throws(new StreetNameHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await Controller.CorrectRetirement(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectRetirementRequest>(),
                new StreetNameCorrectRetirementRequest { PersistentLocalId = Fixture.Create<PersistentLocalId>() },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Message.Contains("Deze actie is enkel toegestaan op straatnamen met status 'gehistoreerd'."))
                .Where(x => x.Errors.Single().ErrorCode == "StraatnaamVoorgesteldOfAfgekeurd");
        }

        [Fact]
        public async Task WhenMunicipalityIsRetired_ThenBadRequestIsReturned()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<StreetNameCorrectRetirementRequest>(), CancellationToken.None))
                .Throws(new MunicipalityHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await Controller.CorrectRetirement(
                MockValidIfMatchValidator(),
                MockPassingRequestValidator<StreetNameCorrectRetirementRequest>(),
                new StreetNameCorrectRetirementRequest { PersistentLocalId = Fixture.Create<PersistentLocalId>() },
                null);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Message.Contains("Deze actie is enkel toegestaan binnen gemeenten met status 'inGebruik'."))
                .Where(x => x.Errors.Single().ErrorCode == "StraatnaamGemeenteInGebruik");
        }
    }
}
