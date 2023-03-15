namespace StreetNameRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentValidation.TestHelper;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Validators;
    using Xunit;

    public sealed class StreetNameCorrectHomonymAdditionsRequestValidatorTests
    {
        private readonly StreetNameCorrectHomonymAdditionsRequestValidator _validator;

        public StreetNameCorrectHomonymAdditionsRequestValidatorTests()
        {
            _validator = new StreetNameCorrectHomonymAdditionsRequestValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("BE")]
        public void GivenValidRequest_ThenValidationSucceeds(string? homonymAddition)
        {
            var result = _validator.TestValidate(new CorrectStreetNameHomonymAdditionsRequest()
            {
                HomoniemToevoegingen = new Dictionary<Taal, string>
                {
                    { Taal.NL,  homonymAddition}
                }
            });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void GivenMaxLenghtExceeded_ThenReturnsExpectedMessage()
        {
            var homonymAddition = "This exceeds the max lenght of 20";
            var result = _validator.TestValidate(new CorrectStreetNameHomonymAdditionsRequest()
            {
                HomoniemToevoegingen = new Dictionary<Taal, string>
                {
                    { Taal.NL,  homonymAddition}
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(CorrectStreetNameHomonymAdditionsRequest.HomoniemToevoegingen)}[0]")
                .WithErrorMessage($"Maximum lengte van een homoniemToevoeging in 'nl' is 20 tekens. U heeft momenteel {homonymAddition.Length} tekens.")
                .WithErrorCode("StraatnaamHomoniemToevoegingMaxlengteValidatie");
        }
    }
}
