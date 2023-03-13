namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Consumer;
    using FluentValidation;

    public sealed class StreetNameChangeNamesRequestValidator : AbstractValidator<ChangeStreetNameNamesRequest>
    {
        public StreetNameChangeNamesRequestValidator(ConsumerContext consumerContext)
        {
            RuleForEach(x => x.Straatnamen)
                .Must(StreetNameNotEmptyValidator.IsValid)
                .WithMessage((_, streetName) => $"Straatnaam in '{streetName.Key.ToString().ToLowerInvariant()}' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);

            RuleForEach(x => x.Straatnamen)
                .Must(StreetNameMaxLengthValidator.IsValid)
                .WithMessage((_, streetName) => $"Maximum lengte van een straatnaam in '{streetName.Key.ToString().ToLowerInvariant()}' is 60 tekens. U heeft momenteel {streetName.Value.Length} tekens.")
                .WithErrorCode(StreetNameMaxLengthValidator.Code);

            RuleFor(x => x.Straatnamen)
                .NotEmpty()
                .WithMessage("De body van het verzoek mag niet leeg.")
                .WithErrorCode("OntbrekendeVerzoekBodyValidatie");
        }
    }
}
