namespace StreetNameRegistry.Importer.Console.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;

    public static class CrabQueries
    {
        public static List<int> GetChangedStraatnaamIdsBetween(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory)
        {
            var straatnaamIds = new List<int>();

            using (var crabEntities = crabEntitiesFactory())
            {
                crabEntities
                    .tblStraatNaam
                    .Where(x => x.beginTijd > since && x.beginTijd <= until)
                    .Select(x => x.straatNaamId)
                    .AddRangeTo(straatnaamIds);

                crabEntities
                    .tblStraatNaam_hist
                    .Where(x => x.beginTijd > since && x.beginTijd <= until)
                    .Select(x => x.straatNaamId.Value)
                    .AddRangeTo(straatnaamIds);

                crabEntities
                    .tblStraatNaam_hist
                    .Where(x => x.eindTijd > since && x.eindTijd <= until && x.eindBewerking == CrabBewerking.Verwijdering.Code)
                    .Select(x => x.straatNaamId.Value)
                    .AddRangeTo(straatnaamIds);

                crabEntities
                    .tblStraatnaamstatus
                    .Where(x => x.begintijd > since && x.begintijd <= until)
                    .Select(x => x.straatnaamid)
                    .AddRangeTo(straatnaamIds);

                crabEntities
                    .tblStraatnaamstatus_hist
                    .Where(x => x.begintijd > since && x.begintijd <= until)
                    .Select(x => x.straatnaamid.Value)
                    .AddRangeTo(straatnaamIds);

                crabEntities
                    .tblStraatnaamstatus_hist
                    .Where(x => x.eindTijd > since && x.eindTijd <= until && x.eindBewerking == CrabBewerking.Verwijdering.Code)
                    .Select(x => x.straatnaamid.Value)
                    .AddRangeTo(straatnaamIds);
            }

            return straatnaamIds                .Distinct()
                .ToList();
        }
    }

    public static class AddRangeToExtension
    {
        public static void AddRangeTo<T>(this IQueryable<T> collection, List<T> list) => list.AddRange(collection);
    }
}
