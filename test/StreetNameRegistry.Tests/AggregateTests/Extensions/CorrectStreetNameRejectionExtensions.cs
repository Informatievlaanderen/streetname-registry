namespace StreetNameRegistry.Tests.AggregateTests.Extensions
{
    using Municipality;
    using Municipality.Commands;

    public static class CorrectStreetNameRejectionExtensions
    {
        public static CorrectStreetNameRejection WithPersistentLocalId(this CorrectStreetNameRejection command, PersistentLocalId persistentLocalId)
        {
            return new CorrectStreetNameRejection(command.MunicipalityId, persistentLocalId, command.Provenance);
        }
    }
}
