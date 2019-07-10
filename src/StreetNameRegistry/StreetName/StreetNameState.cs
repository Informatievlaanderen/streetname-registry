namespace StreetNameRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;

    public partial class StreetName
    {
        private StreetNameId _streetNameId;

        private bool _isCompleted;

        private StreetNameStatus? _status;

        private Language? _primaryLanguage;
        private Language? _secondaryLanguage;
        private PersistentLocalId _persistentLocalId;

        private readonly HomonymAdditions _homonymAdditions = new HomonymAdditions();
        private readonly Names _names = new Names();
        private readonly Chronicle<StreetNameStatusWasImportedFromCrab, int> _statusChronicle = new Chronicle<StreetNameStatusWasImportedFromCrab, int>();

        public bool IsRemoved { get; private set; }

        public Modification LastModificationBasedOnCrab { get; private set; }

        private StreetName()
        {
            Register<StreetNameWasRegistered>(When);
            Register<StreetNameWasRemoved>(When);

            Register<StreetNameWasProposed>(When);
            Register<StreetNameStatusWasRemoved>(When);
            Register<StreetNameWasRetired>(When);
            Register<StreetNameBecameCurrent>(When);
            Register<StreetNameWasCorrectedToCurrent>(When);
            Register<StreetNameWasCorrectedToProposed>(When);
            Register<StreetNameStatusWasCorrectedToRemoved>(When);
            Register<StreetNameWasCorrectedToRetired>(When);

            Register<StreetNameWasNamed>(When);
            Register<StreetNameNameWasCorrected>(When);
            Register<StreetNameNameWasCleared>(When);
            Register<StreetNameNameWasCorrectedToCleared>(When);

            Register<StreetNameHomonymAdditionWasDefined>(When);
            Register<StreetNameHomonymAdditionWasCleared>(When);
            Register<StreetNameHomonymAdditionWasCorrected>(When);
            Register<StreetNameHomonymAdditionWasCorrectedToCleared>(When);

            Register<StreetNamePrimaryLanguageWasDefined>(When);
            Register<StreetNamePrimaryLanguageWasCorrected>(When);
            Register<StreetNamePrimaryLanguageWasCleared>(When);
            Register<StreetNamePrimaryLanguageWasCorrectedToCleared>(When);
            Register<StreetNameSecondaryLanguageWasDefined>(When);
            Register<StreetNameSecondaryLanguageWasCorrected>(When);
            Register<StreetNameSecondaryLanguageWasCleared>(When);
            Register<StreetNameSecondaryLanguageWasCorrectedToCleared>(When);

            Register<StreetNameBecameComplete>(When);
            Register<StreetNameBecameIncomplete>(When);

            Register<StreetNamePersistentLocalIdWasAssigned>(When);

            Register<StreetNameStatusWasImportedFromCrab>(When);
            Register<StreetNameWasImportedFromCrab>(@event => WhenCrabEventApplied(@event.Modification == CrabModification.Delete));
        }

        private void When(StreetNameStatusWasImportedFromCrab @event)
        {
            _statusChronicle.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(StreetNamePersistentLocalIdWasAssigned @event)
        {
            _persistentLocalId = new PersistentLocalId(@event.PersistentLocalId);
        }

        private void When(StreetNameBecameComplete @event)
        {
            _isCompleted = true;
        }

        private void When(StreetNameBecameIncomplete @event)
        {
            _isCompleted = false;
        }

        private void When(StreetNameSecondaryLanguageWasCorrectedToCleared @event)
        {
            _secondaryLanguage = null;
        }

        private void When(StreetNameSecondaryLanguageWasCleared @event)
        {
            _secondaryLanguage = null;
        }

        private void When(StreetNamePrimaryLanguageWasCorrectedToCleared @event)
        {
            _primaryLanguage = null;
        }

        private void When(StreetNamePrimaryLanguageWasCleared @event)
        {
            _primaryLanguage = null;
        }

        private void When(StreetNamePrimaryLanguageWasDefined @event)
        {
            _primaryLanguage = @event.PrimaryLanguage;
        }

        private void When(StreetNamePrimaryLanguageWasCorrected @event)
        {
            _primaryLanguage = @event.PrimaryLanguage;
        }

        private void When(StreetNameSecondaryLanguageWasDefined @event)
        {
            _secondaryLanguage = @event.SecondaryLanguage;
        }

        private void When(StreetNameSecondaryLanguageWasCorrected @event)
        {
            _secondaryLanguage = @event.SecondaryLanguage;
        }

        private void When(StreetNameNameWasCorrected @event)
        {
            _names.AddOrUpdate(@event.Language, @event.Name);
        }

        private void When(StreetNameWasNamed @event)
        {
            _names.AddOrUpdate(@event.Language, @event.Name);
        }

        private void When(StreetNameNameWasCorrectedToCleared @event)
        {
            _names.Remove(@event.Language);
        }

        private void When(StreetNameNameWasCleared @event)
        {
            _names.Remove(@event.Language);
        }

        private void When(StreetNameWasCorrectedToRetired @event)
        {
            _status = StreetNameStatus.Retired;
        }

        private void When(StreetNameStatusWasCorrectedToRemoved @event)
        {
            _status = null;
        }

        private void When(StreetNameWasCorrectedToProposed @event)
        {
            _status = StreetNameStatus.Proposed;
        }

        private void When(StreetNameWasCorrectedToCurrent @event)
        {
            _status = StreetNameStatus.Current;
        }

        private void When(StreetNameWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(StreetNameBecameCurrent @event)
        {
            _status = StreetNameStatus.Current;
        }

        private void When(StreetNameWasRetired @event)
        {
            _status = StreetNameStatus.Retired;
        }

        private void When(StreetNameWasProposed @event)
        {
            _status = StreetNameStatus.Proposed;
        }

        private void When(StreetNameStatusWasRemoved @event)
        {
            _status = null;
        }

        private void When(StreetNameWasRegistered @event)
        {
            _streetNameId = new StreetNameId(@event.StreetNameId);
        }

        private void When(StreetNameHomonymAdditionWasCorrected @event)
        {
            _homonymAdditions.AddOrUpdate(@event.Language, @event.HomonymAddition);
        }

        private void When(StreetNameHomonymAdditionWasDefined @event)
        {
            _homonymAdditions.AddOrUpdate(@event.Language, @event.HomonymAddition);
        }

        private void When(StreetNameHomonymAdditionWasCleared @event)
        {
            _homonymAdditions.Remove(@event.Language);
        }

        private void When(StreetNameHomonymAdditionWasCorrectedToCleared @event)
        {
            _homonymAdditions.Remove(@event.Language);
        }

        private void WhenCrabEventApplied(bool isDeleted = false)
        {
            if (isDeleted)
                LastModificationBasedOnCrab = Modification.Delete;
            else if (LastModificationBasedOnCrab == Modification.Unknown)
                LastModificationBasedOnCrab = Modification.Insert;
            else if (LastModificationBasedOnCrab == Modification.Insert)
                LastModificationBasedOnCrab = Modification.Update;
        }
    }
}
