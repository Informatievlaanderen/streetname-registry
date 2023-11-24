namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using FluentValidation;

    public sealed class RenameStreetNameRequestValidator : AbstractValidator<RenameStreetNameRequest>
    {
        public RenameStreetNameRequestValidator(BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.DoelStraatnaamId)
                .Must(straatNaamId =>
                    OsloPuriValidator.TryParseIdentifier(straatNaamId, out var persistentLocalId) && int.TryParse(persistentLocalId, out _))
                .DependentRules(() =>
                    RuleFor(x => x.DoelStraatnaamId)
                        .MustAsync(async (straatNaamId, ct) =>
                        {
                            OsloPuriValidator.TryParseIdentifier(straatNaamId, out var persistentLocalIdAsString);

                            var persistentLocalId = int.Parse(persistentLocalIdAsString);

                            var municipalityIdByPersistentLocalId = await backOfficeContext
                                .MunicipalityIdByPersistentLocalId
                                .FindAsync(new object?[] { persistentLocalId }, cancellationToken: ct);

                            return municipalityIdByPersistentLocalId is not null;
                        })
                        .WithMessage((_, straatNaamId) => ValidationErrors.Common.StreetNameInvalid.Message(straatNaamId))
                        .WithErrorCode(ValidationErrors.Common.StreetNameInvalid.Code))
                .WithMessage(ValidationErrors.Common.StreetNameNotFound.Message)
                .WithErrorCode(ValidationErrors.Common.StreetNameNotFound.Code);
        }
    }
}
