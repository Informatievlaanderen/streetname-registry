namespace StreetNameRegistry.Importer.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
    using Crab;
    using StreetName.Commands;

    internal class StreetNameCommandGenerator : ICommandGenerator<int>
    {
        private readonly Func<CRABEntities> _crabEntitiesFactory;
        public string Name => GetType().Name;

        public StreetNameCommandGenerator(Func<CRABEntities> crabEntitiesFactory)
        {
            _crabEntitiesFactory = crabEntitiesFactory;
        }

        public IEnumerable<int> GetChangedKeys(DateTime from, DateTime until) =>
            CrabQueries.GetChangedStraatnaamIdsBetween(from, until, _crabEntitiesFactory).Distinct();

        public IEnumerable<dynamic> GenerateInitCommandsFor(int key, DateTime from, DateTime until) =>
            CreateCommands(key, from, until);

        public IEnumerable<dynamic> GenerateUpdateCommandsFor(int key, DateTime from, DateTime until) =>
            CreateCommands(key, from, until);

        private List<dynamic> CreateCommands(int streetNameId, DateTime from, DateTime until)
        {
            var streetNameCommands = new List<ImportStreetNameFromCrab>();
            var streetNameHistCommands = new List<ImportStreetNameFromCrab>();
            var streetNameStatusCommands = new List<ImportStreetNameStatusFromCrab>();
            var streetNameStatusHistCommands = new List<ImportStreetNameStatusFromCrab>();

            using (var context = _crabEntitiesFactory())
            {
                string primaryLanguage, secondaryLanguage, nisCode;

                var straatnaam = StraatnaamQueries.GetTblStraatnaamByStraatnaamId(streetNameId, context);
                var straatnaamHist = StraatnaamQueries.GetTblStraatnaamHistByStraatnaamId(streetNameId, context);

                var gemeenteIds = straatnaamHist
                    .Select(s => s.GemeenteId)
                    .ToList();

                if (straatnaam != null)
                {
                    primaryLanguage = straatnaam.tblGemeente.taalcode;
                    secondaryLanguage = straatnaam.tblGemeente.taalCodeTweedeTaal;
                    nisCode = straatnaam.tblGemeente.NISCode;
                    streetNameCommands.Add(StreetNameMappings.GetCommandFor(straatnaam, primaryLanguage, secondaryLanguage, nisCode));
                    gemeenteIds.Add(straatnaam.GemeenteId);
                }
                else
                {
                    var gemeente = GemeenteQueries.GetTblGemeentesByGemeenteIds(context, new HashSet<int> { straatnaamHist.First().GemeenteId }).First();
                    primaryLanguage = gemeente.taalcode;
                    secondaryLanguage = gemeente.taalCodeTweedeTaal;
                    nisCode = gemeente.NISCode;
                }

                gemeenteIds = gemeenteIds.Distinct().ToList();

                if (gemeenteIds.Count > 2)
                    throw new NotImplementedException();

                streetNameHistCommands.AddRange(StreetNameMappings.GetCommandsFor(straatnaamHist, primaryLanguage, secondaryLanguage, nisCode));
                streetNameStatusCommands.AddRange(StreetNameMappings.GetCommandsFor(StraatnaamQueries.GetTblStraatnaamStatusByStraatnaamId(streetNameId, context)));
                streetNameStatusHistCommands.AddRange(StreetNameMappings.GetCommandsFor(StraatnaamQueries.GetTblStraatnaamStatusHistByStraatnaamId(streetNameId, context)));
            }

            var commandsByStreetnaam = streetNameCommands
                .Concat(streetNameHistCommands)
                .OrderBy(x => x.Timestamp)
                .GroupBy(g => (int)g.StreetNameId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var commands = new List<dynamic>();

            foreach (var streetNameCommand in commandsByStreetnaam)
                commands.Add(streetNameCommand.Value.OrderBy(x => x.Timestamp).First());

            var allStreetNameCommands = streetNameHistCommands.Select(x => Tuple.Create<dynamic, int>(x, 0))
                .Concat(streetNameCommands.Select(x => Tuple.Create<dynamic, int>(x, 1)))
                .Concat(streetNameStatusHistCommands.Select(x => Tuple.Create<dynamic, int>(x, 2)))
                .Concat(streetNameStatusCommands.Select(x => Tuple.Create<dynamic, int>(x, 3)))
                .Where(x => x.Item1.Timestamp > from.ToCrabInstant() && x.Item1.Timestamp <= until.ToCrabInstant())
                .OrderBy(x => x.Item1.Timestamp)
                .ThenBy(x => x.Item2)
                .ToList();

            foreach (var command in allStreetNameCommands)
                commands.Add(command.Item1);

            return commands;
        }
    }
}
