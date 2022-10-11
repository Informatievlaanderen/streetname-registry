namespace StreetNameRegistry.Municipality
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using DataStructures;
    using Events;
    using Exceptions;

    public sealed class MunicipalityStreetName : Entity
    {
        private MunicipalityId _municipalityId;
        private IMunicipalityEvent _lastEvent;

        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;

        public StreetNameStatus Status { get; private set; }
        public HomonymAdditions HomonymAdditions { get; private set; } = new HomonymAdditions();
        public Names Names { get; private set; } = new Names();
        public PersistentLocalId PersistentLocalId { get; private set; }
        public bool IsRemoved { get; private set; }
        public bool IsRetired => Status == StreetNameStatus.Retired;
        public bool IsRejected => Status == StreetNameStatus.Rejected;

        public StreetNameId? LegacyStreetNameId { get; private set; }

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        public MunicipalityStreetName(Action<object> applier)
            : base(applier)
        {
            Register<StreetNameWasMigratedToMunicipality>(When);
            Register<StreetNameWasProposedV2>(When);
            Register<StreetNameWasApproved>(When);
            Register<StreetNameWasCorrectedFromApprovedToProposed>(When);
            Register<StreetNameWasRejected>(When);
            Register<StreetNameWasRetiredV2>(When);
            Register<StreetNameNamesWereCorrected>(When);
        }

        private void When(StreetNameWasMigratedToMunicipality @event)
        {
            _municipalityId = new MunicipalityId(@event.MunicipalityId);
            Status = @event.Status;
            PersistentLocalId = new PersistentLocalId(@event.PersistentLocalId);
            HomonymAdditions = new HomonymAdditions(@event.HomonymAdditions);
            Names = new Names(@event.Names);
            IsRemoved = @event.IsRemoved;
            LegacyStreetNameId = new StreetNameId(@event.StreetNameId);
            _lastEvent = @event;
        }

        private void When(StreetNameWasProposedV2 @event)
        {
            _municipalityId = new MunicipalityId(@event.MunicipalityId);
            Status = StreetNameStatus.Proposed;
            PersistentLocalId = new PersistentLocalId(@event.PersistentLocalId);
            Names = new Names(@event.StreetNameNames);
            IsRemoved = false;
            _lastEvent = @event;
        }

        private void When(StreetNameWasApproved @event)
        {
            Status = StreetNameStatus.Current;
            _lastEvent = @event;
        }

        private void When(StreetNameWasCorrectedFromApprovedToProposed @event)
        {
            Status = StreetNameStatus.Proposed;
            _lastEvent = @event;
        }

        private void When(StreetNameWasRejected @event)
        {
            Status = StreetNameStatus.Rejected;
            _lastEvent = @event;
        }

        private void When(StreetNameWasRetiredV2 @event)
        {
            Status = StreetNameStatus.Retired;
            _lastEvent = @event;
        }

        private void When(StreetNameNamesWereCorrected @event)
        {
            foreach (var streetNameName in @event.StreetNameNames)
            {
                Names.AddOrUpdate(streetNameName.Language, streetNameName.Name);
            }
            _lastEvent = @event;
        }


        public void Approve()
        {
            if (Status == StreetNameStatus.Current)
            {
                return;
            }

            if (Status != StreetNameStatus.Proposed)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasApproved(_municipalityId, PersistentLocalId));
        }


        public void CorrectApproval()
        {
            if (Status == StreetNameStatus.Proposed)
            {
                return;
            }

            if (Status != StreetNameStatus.Current)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasCorrectedFromApprovedToProposed(_municipalityId, PersistentLocalId));
        }

        public void Reject()
        {
            if (Status == StreetNameStatus.Rejected)
            {
                return;
            }

            if (Status != StreetNameStatus.Proposed)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasRejected(_municipalityId, PersistentLocalId));
        }

        public void Retire()
        {
            if (Status == StreetNameStatus.Retired)
            {
                return;
            }

            if (Status != StreetNameStatus.Current)
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            Apply(new StreetNameWasRetiredV2(_municipalityId, PersistentLocalId));
        }

        public void CorrectNames(Names names, Action<Names, PersistentLocalId> guardStreetNameNames)
        {
            var validStatuses = new[] { StreetNameStatus.Proposed, StreetNameStatus.Current };

            if (!validStatuses.Contains(Status))
            {
                throw new StreetNameHasInvalidStatusException(PersistentLocalId);
            }

            guardStreetNameNames(names, PersistentLocalId);

            var correctedNames = new Names(
                names.Where(name => !Names.HasMatch(name.Language, name.Name)));

            if (!correctedNames.Any())
            {
                return;
            }

            Apply(new StreetNameNamesWereCorrected(_municipalityId, PersistentLocalId, correctedNames));
        }

        public void RestoreSnapshot(MunicipalityId municipalityId, StreetNameData streetNameData)
        {
            _municipalityId = municipalityId;

            PersistentLocalId = new PersistentLocalId(streetNameData.StreetNamePersistentLocalId);
            Status = streetNameData.Status;
            IsRemoved = streetNameData.IsRemoved;

            Names = new Names(streetNameData.Names);
            HomonymAdditions = new HomonymAdditions(streetNameData.HomonymAdditions);

            LegacyStreetNameId = streetNameData.LegacyStreetNameId is null
                ? null
                : new StreetNameId(streetNameData.LegacyStreetNameId.Value);

            _lastSnapshotEventHash = streetNameData.LastEventHash;
            _lastSnapshotProvenance = streetNameData.LastProvenanceData;
        }
    }
}
