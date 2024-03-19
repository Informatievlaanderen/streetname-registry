namespace StreetNameRegistry.Tests.BackOffice.Validators
{
    using System;
    using System.Threading.Tasks;
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
        public async Task GivenValidRequest_NoErrorsAreReturned()
        {
            var municipalityId = Guid.NewGuid();
            var persistentLocalId = 10000;
            _backOfficeContext.MunicipalityIdByPersistentLocalId.Add(new MunicipalityIdByPersistentLocalId(
                persistentLocalId,
                municipalityId,
                "NISCODE"));
            _backOfficeContext.SaveChanges();

            var persistentLocalId2 = 10001;
            _backOfficeContext.MunicipalityIdByPersistentLocalId.Add(new MunicipalityIdByPersistentLocalId(
                persistentLocalId2,
                municipalityId,
                "NISCODE"));
            _backOfficeContext.SaveChanges();

            var result = await _validator.TestValidateAsync(new RenameStreetNameRequest
            {
                DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{persistentLocalId}",
                StreetNamePersistentLocalId = persistentLocalId2
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
        public async Task GivenUnknownStreetName_ReturnsExpectedError()
        {
            var puri = "https://data.vlaanderen.be/id/straatnaam/123";
            var result = await _validator.TestValidateAsync(new RenameStreetNameRequest
            {
                DoelStraatnaamId = puri
            });

            result.ShouldHaveValidationErrorFor(nameof(RenameStreetNameRequest.DoelStraatnaamId))
                .WithErrorMessage($"De straatnaam '{puri}' is niet gekend in het straatnaamregister.")
                .WithErrorCode("StraatnaamNietGekendValidatie");
        }

        [Fact]
        public async Task GivenStreetNamesInDifferentMunicipalities_ReturnsExpectedError()
        {
            var persistentLocalId = 10000;
            var persistentLocalId2 = 10001;
            _backOfficeContext.MunicipalityIdByPersistentLocalId.Add(new MunicipalityIdByPersistentLocalId(
                persistentLocalId,
                Guid.NewGuid(),
                "NISCODE"));

            _backOfficeContext.MunicipalityIdByPersistentLocalId.Add(new MunicipalityIdByPersistentLocalId(
                persistentLocalId2,
                Guid.NewGuid(),
                "NISCODE2"));
            _backOfficeContext.SaveChanges();

            var result = await _validator.TestValidateAsync(new RenameStreetNameRequest
            {
                DoelStraatnaamId = $"https://data.vlaanderen.be/id/straatnaam/{persistentLocalId}",
                StreetNamePersistentLocalId = persistentLocalId2
            });

            result.ShouldHaveValidationErrorFor(nameof(RenameStreetNameRequest.DoelStraatnaamId))
                .WithErrorMessage("De meegegeven straatnamen liggen in verschillende gemeenten.")
                .WithErrorCode("StraatnamenAndereGemeenten");
        }
    }
}
