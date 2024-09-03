namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Azure.Core;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using FluentValidation;

    public sealed class RenameStreetNameRequestValidator : AbstractValidator<RenameStreetNameRequest>
    {
        public RenameStreetNameRequestValidator(BackOfficeContext backOfficeContext)
        {
            RuleFor(x => x.DoelStraatnaamId)
                .Must(straatNaamId =>
                    OsloPuriValidator.TryParseIdentifier(straatNaamId, out var persistentLocalId) && int.TryParse(persistentLocalId, out _))
                .DependentRules(() =>
                {
                    RuleFor(x => x.DoelStraatnaamId)
                        .MustAsync(async (request, straatNaamId, ct) =>
                        {
                            OsloPuriValidator.TryParseIdentifier(straatNaamId, out var persistentLocalIdAsString);

                            var persistentLocalId = int.Parse(persistentLocalIdAsString);
                            return persistentLocalId != request.StreetNamePersistentLocalId;
                        })
                        .WithMessage((_, straatNaamId) => ValidationErrors.RenameStreetName.SourceAndDestinationStreetNameAreTheSame.Message(straatNaamId))
                        .WithErrorCode(ValidationErrors.RenameStreetName.SourceAndDestinationStreetNameAreTheSame.Code);

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
                        .DependentRules(() =>
                            RuleFor(y => y.DoelStraatnaamId)
                                .MustAsync(async (request, straatNaamId, ct) =>
                                {
                                    OsloPuriValidator.TryParseIdentifier(straatNaamId, out var persistentLocalIdAsString);

                                    var persistentLocalId = int.Parse(persistentLocalIdAsString);

                                    var municipalityIdByDestinationPersistentLocalId = await backOfficeContext
                                        .MunicipalityIdByPersistentLocalId
                                        .FindAsync(new object?[] { persistentLocalId }, cancellationToken: ct);

                                    var municipalityIdBySourcePersistentLocalId = await backOfficeContext
                                        .MunicipalityIdByPersistentLocalId
                                        .FindAsync(new object?[] { request.StreetNamePersistentLocalId }, cancellationToken: ct);

                                    // Allow nulls to go through, because otherwise the error message would be incorrect
                                    // It will fail in the next rule / domain anyway
                                    return
                                        municipalityIdBySourcePersistentLocalId is null
                                        || municipalityIdByDestinationPersistentLocalId is null
                                        || municipalityIdByDestinationPersistentLocalId.MunicipalityId ==
                                        municipalityIdBySourcePersistentLocalId.MunicipalityId;
                                })
                                .WithMessage(ValidationErrors.RenameStreetName.SourceAndDestinationStreetNameAreNotInSameMunicipality.Message)
                                .WithErrorCode(ValidationErrors.RenameStreetName.SourceAndDestinationStreetNameAreNotInSameMunicipality.Code)
                        )
                        .WithMessage((_, straatNaamId) => ValidationErrors.Common.StreetNameInvalid.Message(straatNaamId))
                        .WithErrorCode(ValidationErrors.Common.StreetNameInvalid.Code);
                })
                .WithMessage(ValidationErrors.Common.StreetNameNotFound.Message)
                .WithErrorCode(ValidationErrors.Common.StreetNameNotFound.Code);
        }
    }
}
