namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using FluentValidation;

    public sealed class StreetNameCorrectHomonymAdditionsRequestValidator : AbstractValidator<CorrectStreetNameHomonymAdditionsRequest>
    {
        public StreetNameCorrectHomonymAdditionsRequestValidator()
        {
            RuleForEach(x => x.HomoniemToevoegingen)
                .Must(h => h.Value is null || h.Value.Length <= 20)
                .WithMessage((_, homonymAddition) => ValidationErrors.CorrectStreetNameHomonymAdditions.HomonymAdditionMaxCharacterLengthExceeded.Message(homonymAddition.Key, homonymAddition.Value.Length))
                .WithErrorCode(ValidationErrors.CorrectStreetNameHomonymAdditions.HomonymAdditionMaxCharacterLengthExceeded.Code);
        }
    }
}
