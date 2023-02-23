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

        [Fact]
        public void GivenMaxLenghtExceeded_ThenReturnsExpectedMessage()
        {
            var result = _validator.TestValidate(new CorrectStreetNameHomonymAdditionsRequest()
            {
                HomoniemToevoegingen = new Dictionary<Taal, string>
                {
                    { Taal.NL, "This exceeds the max lenght of 20" }
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(CorrectStreetNameHomonymAdditionsRequest.HomoniemToevoegingen)}[0]")
                .WithErrorMessage("Homoniemtoevoeging mag maximaal 20 karakters lang zijn.")
                .WithErrorCode("StraatnaamHomoniemMaxlengteValidatie");
        }
    }
}
