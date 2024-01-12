namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public interface ILegacyIdToPersistentLocalIdMapper
    {
        int Find(Guid streetNameId);
    }

    public class LegacyIdToPersistentLocalIdMapper : ILegacyIdToPersistentLocalIdMapper
    {
        private readonly string _eventConnectionString;

        public LegacyIdToPersistentLocalIdMapper(string eventConnectionString)
        {
            _eventConnectionString = eventConnectionString;
        }
        public int Find(Guid streetNameId)
        {
            using var connection = new SqlConnection(_eventConnectionString);
            connection.Open();
            var result = connection.QuerySingle(@$"
                        SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""PersistentLocalId""
                        FROM [streetname-registry-events].[StreetNameRegistry].[Streams] as s
                        INNER JOIN [streetname-registry-events].[StreetNameRegistry].[Messages] as m
                        ON s.IdInternal = m.StreamIdInternal AND m.[Type] = 'StreetNamePersistentLocalIdentifierWasAssigned'
                        where IdOriginal = '{streetNameId:D}'");

            return result is not null
                ? int.Parse(result?.PersistentLocalId)
                : throw new InvalidOperationException($"Could not find persistentLocalId for '{streetNameId:D}'");
        }
    }
}
