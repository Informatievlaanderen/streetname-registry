namespace StreetNameRegistry.Tests.BackOffice.Validators
{
    using System;
    using FluentValidation.TestHelper;
    using StreetNameRegistry.Api.BackOffice.Abstractions;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Validators;
    using Xunit;

    public sealed class RenameStreetNameRequestValidatorTests
    {
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly RenameStreetNameRequestValidator _validator;

        public RenameStreetNameRequestValidatorTests()
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _validator = new RenameStreetNameRequestValidator(_backOfficeContext);
        }

        [Fact]
        public void GivenValidRequest_NoErrorsAreReturned()
        {
            var persistentLocalId = 10000;
            _backOfficeContext.MunicipalityIdByPersistentLocalId.Add(new MunicipalityIdByPersistentLocalId(
                persistentLocalId,
                Guid.NewGuid(),
                "NISCODE"));
            _backOfficeContext.SaveChanges();

            var result = _validator.TestValidate(new RenameStreetNameRequest
            {
                DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{persistentLocalId}"
            });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData($"https://google.be")]
        [InlineData($"https://data.vlaanderen.be/id/straatnaam/ABC")]
        public void GivenInvalidPuri_ReturnsExpectedError(string invalidPuri)
        {
            var result = _validator.TestValidate(new RenameStreetNameRequest
            {
                DoelStraatnaamId = invalidPuri
            });

            result.ShouldHaveValidationErrorFor(nameof(RenameStreetNameRequest.DoelStraatnaamId))
                .WithErrorMessage("Onbestaande straatnaam.")
                .WithErrorCode("OnbestaandeStraatnaam");
        }

        [Fact]
        public void WithUnknownStreetName_ReturnsExpectedError()
        {
            var puri = "https://data.vlaanderen.be/id/straatnaam/123";
            var result = _validator.TestValidate(new RenameStreetNameRequest
            {
                DoelStraatnaamId = puri
            });

            result.ShouldHaveValidationErrorFor(nameof(RenameStreetNameRequest.DoelStraatnaamId))
                .WithErrorMessage($"De straatnaam '{puri}' is niet gekend in het straatnaamregister.")
                .WithErrorCode("StraatnaamNietGekendValidatie");
        }

    }
}
