namespace StreetNameRegistry.Municipality
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class MunicipalityStreetName
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
            Register<StreetNameWasRejected>(When);
            Register<StreetNameWasRetiredV2>(When);
            Register<StreetNameNamesWereCorrected>(When);
            Register<StreetNameWasCorrectedFromApprovedToProposed>(When);
            Register<StreetNameWasCorrectedFromRejectedToProposed>(When);
            Register<StreetNameWasCorrectedFromRetiredToCurrent>(When);
            Register<StreetNameHomonymAdditionsWereCorrected>(When);
            Register<StreetNameHomonymAdditionsWereRemoved>(When);
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

        private void When(StreetNameWasCorrectedFromApprovedToProposed @event)
        {
            Status = StreetNameStatus.Proposed;
            _lastEvent = @event;
        }

        private void When(StreetNameWasCorrectedFromRejectedToProposed @event)
        {
            Status = StreetNameStatus.Proposed;
            _lastEvent = @event;
        }

        private void When(StreetNameWasCorrectedFromRetiredToCurrent @event)
        {
            Status = StreetNameStatus.Current;
            _lastEvent = @event;
        }

        private void When(StreetNameHomonymAdditionsWereCorrected @event)
        {
            foreach(var (language, homonymAddition) in @event.HomonymAdditions)
            {
                HomonymAdditions.AddOrUpdate(language, homonymAddition);
            }

            _lastEvent = @event;
        }

        private void When(StreetNameHomonymAdditionsWereRemoved @event)
        {
            foreach(var language in @event.Languages)
            {
                HomonymAdditions.Remove(language);
            }

            _lastEvent = @event;
        }
    }
}
