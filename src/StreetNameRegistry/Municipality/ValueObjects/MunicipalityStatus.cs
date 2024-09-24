namespace StreetNameRegistry.Municipality
{
    using System;

    public readonly struct MunicipalityStatus
    {
        public static readonly MunicipalityStatus Current = new MunicipalityStatus("Current");
        public static readonly MunicipalityStatus Retired = new MunicipalityStatus("Retired");
        public static readonly MunicipalityStatus Proposed = new MunicipalityStatus("Proposed");

        public string Status { get; }

        private MunicipalityStatus(string status) => Status = status;

        public static MunicipalityStatus Parse(string status)
        {
            if (status != Retired.Status &&
                status != Current.Status &&
                status != Proposed.Status)
                throw new NotImplementedException($"Cannot parse '{status}' to {nameof(MunicipalityStatus)}");

            return new MunicipalityStatus(status);
        }

        public static implicit operator string(MunicipalityStatus status) => status.Status;
    }
}
