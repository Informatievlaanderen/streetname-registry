namespace StreetNameRegistry.StreetName
{
    using System.Collections.Generic;
    using Events;

    public partial class Municipality
    {
        private MunicipalityId _municipalityId;
        private Names _streetNameNames;
        private NisCode _nisCode;
        private readonly List<Language> _officialLanguages = new();
        private readonly List<Language> _facilityLanguages = new();

        internal MunicipalityId MunicipalityId => _municipalityId;

        public MunicipalityStatus MunicipalityStatus { get; private set; }

        private Municipality()
        {
            Register<MunicipalityWasImported>(When);
            Register<MunicipalityWasRetired>(When);
            Register<MunicipalityNisCodeWasChanged>(When);
            Register<MunicipalityBecameCurrent>(When);

            Register<MunicipalityFacilityLanguageWasRemoved>(When);
            Register<MunicipalityFacilityLanguageWasAdded>(When);
            Register<MunicipalityOfficialLanguageWasAdded>(When);
            Register<MunicipalityOfficialLanguageWasRemoved>(When);
            Register<MunicipalityWasCorrectedToCurrent>(When);
            Register<MunicipalityWasCorrectedToRetired>(When);

            Register<StreetNameWasProposedV2>(When);
        }

        #region Municipality
        private void When(MunicipalityWasImported @event)
        {
            _municipalityId = new MunicipalityId(@event.MunicipalityId);
            _nisCode = new NisCode(@event.NisCode);
        }

        private void When(MunicipalityWasCorrectedToCurrent @event)
        {
            if (MunicipalityStatus != MunicipalityStatus.Current)
                MunicipalityStatus = MunicipalityStatus.Current;
        }

        private void When(MunicipalityWasCorrectedToRetired @event)
        {
            if (MunicipalityStatus != MunicipalityStatus.Retired)
                MunicipalityStatus = MunicipalityStatus.Retired;
        }

        private void When(MunicipalityWasRetired @event)
        {
            MunicipalityStatus = MunicipalityStatus.Retired;
        }

        private void When(MunicipalityBecameCurrent @event)
        {
            MunicipalityStatus = MunicipalityStatus.Current;
        }

        private void When(MunicipalityNisCodeWasChanged @event)
        {
            _nisCode = new NisCode(@event.NisCode);
        }

        private void When(MunicipalityFacilityLanguageWasAdded @event)
        {
            _facilityLanguages.Add(@event.Language);
        }

        private void When(MunicipalityFacilityLanguageWasRemoved @event)
        {
            _facilityLanguages.Remove(@event.Language);
        }

        private void When(MunicipalityOfficialLanguageWasAdded @event)
        {
            _officialLanguages.Add(@event.Language);
        }

        private void When(MunicipalityOfficialLanguageWasRemoved @event)
        {
            _officialLanguages.Remove(@event.Language);
        }
        #endregion Municipality

        private void When(StreetNameWasProposedV2 @event)
        {
            _streetNameNames = new Names(@event.StreetNameNames);
        }

    }
}
