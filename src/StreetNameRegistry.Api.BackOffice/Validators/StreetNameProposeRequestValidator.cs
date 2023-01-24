namespace StreetNameRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Consumer;
    using FluentValidation;

    public sealed class StreetNameProposeRequestValidator : AbstractValidator<ProposeStreetNameRequest>
    {
        public StreetNameProposeRequestValidator(ConsumerContext consumerContext)
        {
            RuleFor(x => x.GemeenteId)
                .MustAsync((municipalityId, cancellationToken) => new StreetNameExistingNisCodeValidator(consumerContext).IsValidAsync(municipalityId, cancellationToken))
                .WithMessage(request => $"De gemeente '{request.GemeenteId}' is niet gekend in het gemeenteregister.")
                .WithErrorCode(StreetNameExistingNisCodeValidator.Code);

            RuleFor(x => x.GemeenteId)
                .MustAsync((municipalityId, cancellationToken) => new StreetNameFlemishRegionValidator(consumerContext).IsValidAsync(municipalityId, cancellationToken))
                .WithMessage(request => $"De gemeente '{request.GemeenteId}' is geen Vlaamse gemeente.")
                .WithErrorCode(StreetNameFlemishRegionValidator.Code);

            RuleForEach(x => x.Straatnamen)
                .Must(StreetNameNotEmptyValidator.IsValid)
                .WithMessage((_, streetName) => $"Straatnaam in '{streetName.Key.ToString().ToLowerInvariant()}' kan niet leeg zijn.")
                .WithErrorCode(StreetNameNotEmptyValidator.Code);

            RuleForEach(x => x.Straatnamen)
                .Must(StreetNameMaxLengthValidator.IsValid)
                .WithMessage((_, streetName) => $"Maximum lengte van een straatnaam in '{streetName.Key.ToString().ToLowerInvariant()}' is 60 tekens. U heeft momenteel {streetName.Value.Length} tekens.")
                .WithErrorCode(StreetNameMaxLengthValidator.Code);
        }
    }
}
