namespace StreetNameRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public sealed class ImportStreetNameStatusFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("59313d3d-269b-4e55-9345-672b69536d51");

        public CrabStreetNameStatusId StreetNameStatusId { get; }
        public CrabStreetNameId StreetNameId { get; }
        public CrabStreetNameStatus StreetNameStatus { get; }
        public CrabLifetime LifeTime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportStreetNameStatusFromCrab(
            CrabStreetNameStatusId streetNameStatusId,
            CrabStreetNameId streetNameId,
            CrabStreetNameStatus streetNameStatus,
            CrabLifetime lifeTime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            StreetNameStatusId = streetNameStatusId;
            StreetNameId = streetNameId;
            StreetNameStatus = streetNameStatus;
            LifeTime = lifeTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportStreetNameStatusFromCrab-{ToString()}");

        public override string ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNameStatusId;
            yield return StreetNameId;
            yield return StreetNameStatus;
            yield return LifeTime.BeginDateTime.Print();
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
