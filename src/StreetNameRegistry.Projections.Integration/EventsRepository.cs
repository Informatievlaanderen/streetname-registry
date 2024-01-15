namespace StreetNameRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class EventsRepository : IEventsRepository

    {
        private readonly string _eventsConnectionString;

        public EventsRepository(string eventsConnectionString)
        {
            _eventsConnectionString = eventsConnectionString;
        }

        public async Task<int?> GetPersistentLocalId(Guid addressId)
        {
            await using var connection = new SqlConnection(_eventsConnectionString);
            var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""PersistentLocalId""
                    FROM [streetname-registry-events].[StreetNameRegistry].[Streams] as s
                    inner join [streetname-registry-events].[StreetNameRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'StreetNamePersistentLocalIdentifierWasAssigned'
                    where s.Id = '{addressId}'";

            return await connection.QuerySingleOrDefaultAsync<int?>(sql);
        }
    }
}
