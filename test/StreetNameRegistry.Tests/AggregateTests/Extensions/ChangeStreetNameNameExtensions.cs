namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using System.Linq;
    using Municipality;
    using Municipality.Commands;

    public static class ChangeStreetNameNameExtensions
    {
        public static ChangeStreetNameNames WithMunicipalityId(this ChangeStreetNameNames command, MunicipalityId municipalityId)
        {
            return new ChangeStreetNameNames(municipalityId, command.PersistentLocalId, command.StreetNameNames, command.Provenance);
        }

        public static ChangeStreetNameNames WithStreetNameName(this ChangeStreetNameNames command, StreetNameName name)
        {
            var names = new Names(command.StreetNameNames);
            if (command.StreetNameNames.HasLanguage(name.Language))
            {
                names.Remove(name.Language);
            }

            names = new Names(names.Concat(new[] { name }));

            return new ChangeStreetNameNames(command.MunicipalityId, command.PersistentLocalId, names, command.Provenance);
        }

        public static ChangeStreetNameNames WithStreetNameNames(this ChangeStreetNameNames command, Names names)
        {
            return new ChangeStreetNameNames(
                command.MunicipalityId,
                command.PersistentLocalId,
                names,
                command.Provenance);
        }

        public static ChangeStreetNameNames WithPersistentLocalId(this ChangeStreetNameNames command, PersistentLocalId persistentLocalId)
        {
            return new ChangeStreetNameNames(command.MunicipalityId, persistentLocalId, command.StreetNameNames, command.Provenance);
        }
    }
}
