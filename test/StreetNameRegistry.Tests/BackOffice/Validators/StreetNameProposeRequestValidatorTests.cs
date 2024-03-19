namespace StreetNameRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentValidation.TestHelper;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Validators;
    using Xunit;

    public sealed class StreetNameProposeRequestValidatorTests
    {
        private readonly TestConsumerContext _consumerContext;
        private readonly StreetNameProposeRequestValidator _validator;

        public StreetNameProposeRequestValidatorTests()
        {
            _consumerContext = new FakeConsumerContextFactory().CreateDbContext();
            _validator = new StreetNameProposeRequestValidator(_consumerContext);
        }

        [Fact]
        public async Task GivenEmptyStreetName_ThenReturnsExpectedMessage()
        {
            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = "bla",
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.Straatnamen)}[0]")
                .WithErrorMessage($"Straatnaam in 'nl' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);
        }

        [Fact]
        public async Task GivenOneEmptyStreetName_ThenReturnsExpectedMessage()
        {
            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = "bla",
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "teststraat"},
                    { Taal.FR, "" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.Straatnamen)}[1]")
                .WithErrorMessage("Straatnaam in 'fr' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);
        }

        [Fact]
        public async Task GivenStreetNameExceededMaxLength_ThenReturnsExpectedMessage()
        {
            const string streetName = "Boulevard Louis Edelhart Lodewijk van Groothertogdom Luxemburg";

            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = "bla",
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, streetName }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.Straatnamen)}[0]")
                .WithErrorMessage($"Maximum lengte van een straatnaam in 'nl' is 60 tekens. U heeft momenteel {streetName.Length} tekens.")
                .WithErrorCode(StreetNameMaxLengthValidator.Code);
        }

        [Fact]
        public async Task GivenNisCodeIsInvalidPuri_ThenReturnsExpectedMessage()
        {
            const string gemeenteId = "bla";

            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = gemeenteId,
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "Sint-Niklaasstraat" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.GemeenteId)}")
                .WithErrorMessage($"De gemeente '{gemeenteId}' is niet gekend in het gemeenteregister.")
                .WithErrorCode(StreetNameExistingNisCodeValidator.Code);
        }

        [Fact]
        public async Task GivenNonExistentNisCode_ThenReturnsExpectedMessage()
        {
            const string gemeenteId = "https://data.vlaanderen.be/id/gemeente/bla";

            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = gemeenteId,
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "Sint-Niklaasstraat" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.GemeenteId)}")
                .WithErrorMessage($"De gemeente '{gemeenteId}' is niet gekend in het gemeenteregister.")
                .WithErrorCode(StreetNameExistingNisCodeValidator.Code);
        }

        [Fact]
        public async Task GivenNonFlemishNisCode_ThenReturnsExpectedMessage()
        {
            const string nisCode = "55001";
            const string gemeenteId = $"https://data.vlaanderen.be/id/gemeente/{nisCode}";

            _consumerContext.AddMunicipalityLatestItemFixtureWithNisCode(nisCode);
            var result = await _validator.TestValidateAsync(new ProposeStreetNameRequest
            {
                GemeenteId = gemeenteId,
                Straatnamen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "Sint-Niklaasstraat" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(ProposeStreetNameRequest.GemeenteId)}")
                .WithErrorMessage($"De gemeente '{gemeenteId}' is geen Vlaamse gemeente.")
                .WithErrorCode(StreetNameFlemishRegionValidator.Code);
        }
    }
}
