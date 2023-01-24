namespace StreetNameRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentValidation.TestHelper;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Validators;
    using Xunit;

    public sealed class StreetNameCorrectNamesRequestValidatorTests
    {
        private readonly StreetNameCorrectNamesRequestValidator _validator;

        public StreetNameCorrectNamesRequestValidatorTests()
        {
            var consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _validator = new StreetNameCorrectNamesRequestValidator(consumerContext);
        }

        [Fact]
        public void GivenEmptyStreetName_ThenReturnsExpectedMessage()
        {
            var result = _validator.TestValidate(new CorrectStreetNameNamesRequest()
            {
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(CorrectStreetNameNamesRequest.Straatnamen)}[0]")
                .WithErrorMessage($"Straatnaam in 'nl' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);
        }

        [Fact]
        public void GivenOneEmptyStreetName_ThenReturnsExpectedMessage()
        {
            var result = _validator.TestValidate(new CorrectStreetNameNamesRequest()
            {
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "teststraat"},
                    { Taal.FR, "" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(CorrectStreetNameNamesRequest.Straatnamen)}[1]")
                .WithErrorMessage("Straatnaam in 'fr' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);
        }

        [Fact]
        public void GivenStreetNameExceededMaxLength_ThenReturnsExpectedMessage()
        {
            const string streetName = "Boulevard Louis Edelhart Lodewijk van Groothertogdom Luxemburg";

            var result = _validator.TestValidate(new CorrectStreetNameNamesRequest
            {
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, streetName }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(CorrectStreetNameNamesRequest.Straatnamen)}[0]")
                .WithErrorMessage($"Maximum lengte van een straatnaam in 'nl' is 60 tekens. U heeft momenteel {streetName.Length} tekens.")
                .WithErrorCode(StreetNameMaxLengthValidator.Code);
        }

        [Fact]
        public void GivenStreetNamesIsNull_ThenReturnsExpectedMessage()
        {
            var result = _validator.TestValidate(new CorrectStreetNameNamesRequest
            {
                Straatnamen = null
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectStreetNameNamesRequest.Straatnamen))
                .WithErrorCode("OntbrekendeVerzoekBodyValidatie")
                .WithErrorMessage("De body van het verzoek mag niet leeg.");
        }

        [Fact]
        public void GivenEmptyStreetNamesList_ThenReturnsExpectedMessage()
        {
            var result = _validator.TestValidate(new CorrectStreetNameNamesRequest
            {
                Straatnamen = new Dictionary<Taal, string>()
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectStreetNameNamesRequest.Straatnamen))
                .WithErrorCode("OntbrekendeVerzoekBodyValidatie")
                .WithErrorMessage("De body van het verzoek mag niet leeg.");
        }
    }
}
