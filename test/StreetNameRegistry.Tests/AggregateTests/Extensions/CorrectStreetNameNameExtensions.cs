namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using System.Linq;
    using Municipality;
    using Municipality.Commands;

    public static class CorrectStreetNameNameExtensions
    {
        public static CorrectStreetNameNames WithMunicipalityId(this CorrectStreetNameNames command, MunicipalityId municipalityId)
        {
            return new CorrectStreetNameNames(municipalityId, command.PersistentLocalId, command.StreetNameNames, command.Provenance);
        }

        public static CorrectStreetNameNames WithStreetNameName(this CorrectStreetNameNames command, StreetNameName name)
        {
            var names = new Names(command.StreetNameNames);
            if (command.StreetNameNames.HasLanguage(name.Language))
            {
                names.Remove(name.Language);
            }

            names = new Names(names.Concat(new[] { name }));

            return new CorrectStreetNameNames(command.MunicipalityId, command.PersistentLocalId, names, command.Provenance);
        }

        public static CorrectStreetNameNames WithStreetNameNames(this CorrectStreetNameNames command, Names names)
        {
            return new CorrectStreetNameNames(
                command.MunicipalityId,
                command.PersistentLocalId,
                names,
                command.Provenance);
        }

        public static CorrectStreetNameNames WithPersistentLocalId(this CorrectStreetNameNames command, PersistentLocalId persistentLocalId)
        {
            return new CorrectStreetNameNames(command.MunicipalityId, persistentLocalId, command.StreetNameNames, command.Provenance);
        }
    }
}
