namespace StreetNameRegistry.Municipality
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using NodaTime;

    public sealed class RetirementDate : InstantValueObject<RetirementDate>
    {
        public RetirementDate(Instant instant)
            : base(instant)
        { }
    }
}
