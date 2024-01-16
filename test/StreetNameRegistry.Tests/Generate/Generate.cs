namespace StreetNameRegistry.Tests.Generate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using NodaTime;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using StreetName;
    using StreetName.Events;

    [Obsolete("Do not use this class, try with autofixture and/or look at projection V2 tests")]
    public static class Generate
    {
        public static Generator<StreetNameId> StreetNameId =
            new Generator<StreetNameId>(r => new StreetNameId(Produce.Guid().Generate(r)));

        public static Generator<CrabStreetNameStatusId> CrabStreetNameStatusId =
            new Generator<CrabStreetNameStatusId>(r => new CrabStreetNameStatusId(Produce.Integer().Generate(r)));

        public static Generator<CrabStreetNameId> CrabStreetNameId =
            new Generator<CrabStreetNameId>(r => new CrabStreetNameId(Produce.Integer().Generate(r)));

        public static Generator<CrabLifetime> CrabLifetime = new Generator<CrabLifetime>(r =>
            new CrabLifetime(Produce.LocalDateTime().Generate(r), Produce.LocalDateTime().Select(date => date.PlusDays(1)).Generate(r)));

        public static Generator<CrabMunicipalityId> CrabMunicipalityId =
            new Generator<CrabMunicipalityId>(r => new CrabMunicipalityId(Produce.Integer().Generate(r)));

        public static Generator<NisCode> NisCode =
            new Generator<NisCode>(r => new NisCode(NisCodeString.Generate(r)));

        public static Generator<CrabStreetName> CrabStreetName =
            new Generator<CrabStreetName>(r => new CrabStreetName(StreetNameString.Generate(r), CrabLanguageNullable.Generate(r)));

        public static Generator<CrabTransStreetName> CrabTransStreetName =
            new Generator<CrabTransStreetName>(r => new CrabTransStreetName(Produce.LowerCaseString().Generate(r), CrabLanguageNullable.Generate(r)));

        public static Generator<CrabTimestamp> CrabTimestamp =
            new Generator<CrabTimestamp>(r => new CrabTimestamp(Produce.Instant().Generate(r)));

        public static Generator<CrabOperator> CrabOperator =
            new Generator<CrabOperator>(r => new CrabOperator(Produce.AlphaNumericString(10).Generate(r)));

        public static Generator<CrabModification?> CrabModificationNullable =
            NullableEnum<CrabModification>(); //.Select(x=> new CrabModificationNullable?(Be.Vlaanderen.Basisregisters.Crab.ValueObjects.CrabModificationNullable.Delete));

        public static Generator<CrabModification?> CrabModificationNullableExceptDeleteCorrection =
            NullableEnumExcept<CrabModification>(
                CrabModification.Delete,
                CrabModification.Correction);

        public static Generator<CrabOrganisation?> CrabOrganisationNullable = NullableEnum<CrabOrganisation>();

        public static Generator<CrabStreetNameStatus> CrabStreetNameStatus = Enum<CrabStreetNameStatus>();

        public static Generator<Language?> LanguageNullable = NullableEnum<Language>();

        public static Generator<Language> Language = Enum<Language>();

        public static Generator<CrabLanguage?> CrabLanguageNullable = NullableEnum<CrabLanguage>();

        public static Generator<CrabLanguage> CrabLanguage = Enum<CrabLanguage>();

        public static Generator<string> StreetNameString = new Generator<string>(r =>
            Produce.LowerCaseString().Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1) + "straat").Generate(r));

        public static Generator<string> NisCodeString = new Generator<string>(r => Produce.NumericString(6).Generate(r));

        public static Generator<StreetNameName> StreetNameName = new Generator<StreetNameName>(r => new StreetNameName(StreetNameString.Generate(r), Language.Generate(r)));

        public static Generator<MunicipalityId> MunicipalityId = new Generator<MunicipalityId>(r => new MunicipalityId(Produce.Guid().Generate(r)));

        public static Generator<StreetNameWasNamed> StreetNameNameWasNamed = new Generator<StreetNameWasNamed>(r =>
            new StreetNameWasNamed(StreetNameId.Generate(r), StreetNameName.Generate(r)));

        public static Generator<StreetNameWasRemoved> StreetNameWasRemoved = new Generator<StreetNameWasRemoved>(r =>
            new StreetNameWasRemoved(StreetNameId.Generate(r)));

        public static Generator<StreetNameWasRetired> StreetNameWasRetired = new Generator<StreetNameWasRetired>(r =>
            new StreetNameWasRetired(StreetNameId.Generate(r)));

        public static Generator<StreetNameNameWasCleared> StreetNameNameWasCleared = new Generator<StreetNameNameWasCleared>(r =>
            new StreetNameNameWasCleared(StreetNameId.Generate(r), LanguageNullable.Generate(r)));

        public static Generator<StreetNameWasRegistered> StreetNameWasRegistered = new Generator<StreetNameWasRegistered>(r =>
            new StreetNameWasRegistered(StreetNameId.Generate(r), MunicipalityId.Generate(r), NisCode.Generate(r)));

        public static Generator<StreetNamePersistentLocalIdWasAssigned> StreetNamePersistentLocalIdWasAssigned = new Generator<StreetNamePersistentLocalIdWasAssigned>(r =>
            new StreetNamePersistentLocalIdWasAssigned(StreetNameId.Generate(r), new PersistentLocalId(Produce.Integer(10000, 15000).Generate(r)), new PersistentLocalIdAssignmentDate(Produce.Instant().Generate(r))));

        public static Generator<StreetNameBecameComplete> StreetNameBecameComplete = new Generator<StreetNameBecameComplete>(r =>
            new StreetNameBecameComplete(StreetNameId.Generate(r)));

        public static Generator<CrabStreetName> CrabStreetNameExceptLanguage(CrabLanguage? language)
        {
            return new Generator<CrabStreetName>(r => new CrabStreetName(StreetNameString.Generate(r), NullableEnumExcept(language).Generate(r)));
        }

        public static Generator<CrabStreetName> CrabStreetNameWithName(string name)
        {
            return new Generator<CrabStreetName>(r => new CrabStreetName(name, CrabLanguageNullable.Generate(r)));
        }

        public static Generator<CrabStreetName> CrabStreetNameWithNameAndLanguage(string name, CrabLanguage? language)
        {
            return new Generator<CrabStreetName>(r => new CrabStreetName(name, language));
        }

        public static Generator<CrabStreetName> CrabStreetNameWithLanguage(CrabLanguage? language)
        {
            return new Generator<CrabStreetName>(r => CrabStreetName.Select(x => new CrabStreetName(x.Name, language)).Generate(r));
        }

        public static Generator<CrabLifetime> CrabLifetimeWithEndDate(DateTime? endDate)
        {
            return new Generator<CrabLifetime>(r => new CrabLifetime(Produce.LocalDateTime().Generate(r), endDate?.ToCrabLocalDateTime()));
        }

        public static Generator<Provenance> Provenance = new Generator<Provenance>(r =>
        {
            var provenance = new Provenance(
                Instant.FromDateTimeOffset(DateTimeOffset.Now),
                (Application)Produce.Integer(1, 5).Generate(r),
                new Reason(Produce.AlphaNumericString(5, 15).Generate(r)),
                new Operator(Produce.AlphaNumericString(5, 15).Generate(r)),
                (Modification)Produce.Integer(1, 3).Generate(r),
                (Organisation)Produce.Integer(1, 10).Generate(r));

            return provenance;
        });

        public static Generator<AtomEntry> AtomEntry<TEvent>(TEvent @event, long id)
            where TEvent : struct
        {
            return new Generator<AtomEntry>(r =>
            {
                var atomEntry = new Microsoft.SyndicationFeed.Atom.AtomEntry
                {
                    Id = id.ToString(),
                    Title = $"{@event}-{id}",
                    LastUpdated = Produce.Date().Generate(r)
                };

                return new AtomEntry(atomEntry, new SyndicationContent<Gemeente> { Object = new Gemeente() });
            });
        }

        public static Generator<TEnum?> NullableEnum<TEnum>()
            where TEnum : struct, IConvertible
        {
            return new Generator<TEnum?>(r =>
            {
                var values = System.Enum.GetValues(typeof(TEnum)).Cast<int>().ToList();
                var min = values.Min();
                var max = values.Max();
                var chosen = Produce.Integer(min, max + 2).Generate(r);
                return chosen > max ? null : new TEnum?(System.Enum.Parse<TEnum>(chosen.ToString()));
            });
        }

        public static Generator<TEnum> Enum<TEnum>()
            where TEnum : struct, IConvertible
        {
            return new Generator<TEnum>(r =>
            {
                var values = System.Enum.GetValues(typeof(TEnum)).Cast<int>().ToList();
                var min = values.Min();
                var max = values.Max();
                var chosen = Produce.Integer(min, max + 1).Generate(r);
                return System.Enum.Parse<TEnum>(chosen.ToString());
            });
        }

        public static Generator<TEnum?> NullableEnumExcept<TEnum>(params TEnum?[] exceptions)
            where TEnum : struct, IConvertible
        {
            return new Generator<TEnum?>(r =>
            {
                var values = System.Enum.GetValues(typeof(TEnum)).Cast<int>();

                if (values.Count() == exceptions.Distinct().Count())
                    throw new ArgumentException("Cannot except all possible values");

                TEnum? chosen;
                while (exceptions.Contains(chosen = NullableEnum<TEnum>().Generate(r)))
                {
                }

                return chosen;
            });
        }

        public static Generator<TEnum> EnumExcept<TEnum>(params TEnum?[] exceptions)
            where TEnum : struct, IConvertible
        {
            return new Generator<TEnum>(r =>
            {
                var values = System.Enum.GetValues(typeof(TEnum)).Cast<int>();

                if (values.Count() == exceptions.Distinct().Count())
                    throw new ArgumentException("Cannot except all possible values");

                TEnum chosen;
                while (exceptions.Contains(chosen = Enum<TEnum>().Generate(r)))
                {
                }

                return chosen;
            });
        }

        public static Generator<string> StreetNameWithHomonymString(string homonymAddition)
        {
            return new Generator<string>(r =>
                StreetNameString.Select(s => s + "_" + homonymAddition).Generate(r));
        }

        public static Generator<T> This<T>(T t) => new Generator<T>(r => t);

        public static class EventsFor
        {
            public static Generator<IEnumerable<IMessage>> StreetName(Guid streetNameId, string nameDutch = null)
            {
                var provenance = new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now),
                    (Application)Produce.Integer(1, 5).Generate(new Random()),
                    new Reason(Produce.AlphaNumericString(5, 15).Generate(new Random())),
                    new Operator(Produce.AlphaNumericString(5, 15).Generate(new Random())),
                    (Modification)Produce.Integer(1, 3).Generate(new Random()),
                    (Organisation)Produce.Integer(1, 10).Generate(new Random()));

                return new Generator<IEnumerable<IMessage>>(r =>
                {
                    var events = new List<IGenerator<IMessage>>
                    {
                        StreetNameWasRegistered
                            .Select(e =>
                                e.WithId(streetNameId)
                                 .WithProvenance(provenance))
                    };

                    if (nameDutch != null)
                        events.Add(StreetNameNameWasNamed
                            .Select(e => e.WithId(streetNameId)
                                .WithName(nameDutch)
                                .WithLanguage(StreetNameRegistry.StreetName.Language.Dutch)
                                .WithProvenance(provenance)));

                    return events.Select(e => e.Generate(r));
                });
            }
        }
    }

    public static class Modifiers
    {
        public static StreetNameWasRegistered WithId(this StreetNameWasRegistered e, Guid id)
        {
            return new StreetNameWasRegistered(new StreetNameId(id), new MunicipalityId(e.MunicipalityId), new NisCode(e.NisCode));
        }

        public static StreetNameWasRegistered WithNisCode(this StreetNameWasRegistered e, string nisCode)
        {
            return new StreetNameWasRegistered(new StreetNameId(e.StreetNameId), new MunicipalityId(e.MunicipalityId), new NisCode(nisCode));
        }

        public static StreetNameWasRegistered WithProvenance(this StreetNameWasRegistered e, Provenance provenance)
        {
            var newEvent = new StreetNameWasRegistered(new StreetNameId(e.StreetNameId), new MunicipalityId(e.MunicipalityId), new NisCode(e.NisCode));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNamePersistentLocalIdWasAssigned WithId(this StreetNamePersistentLocalIdWasAssigned e, Guid id)
        {
            return new StreetNamePersistentLocalIdWasAssigned(new StreetNameId(id), new PersistentLocalId(e.PersistentLocalId), new PersistentLocalIdAssignmentDate(e.AssignmentDate));
        }

        public static StreetNamePersistentLocalIdWasAssigned WithPersistentLocalId(this StreetNamePersistentLocalIdWasAssigned e, int persistentLocalId)
        {
            return new StreetNamePersistentLocalIdWasAssigned(new StreetNameId(e.StreetNameId), new PersistentLocalId(persistentLocalId), new PersistentLocalIdAssignmentDate(e.AssignmentDate));
        }

        public static StreetNameWasNamed WithId(this StreetNameWasNamed e, Guid id)
        {
            return new StreetNameWasNamed(new StreetNameId(id), new StreetNameName(e.Name, e.Language));
        }

        public static StreetNameWasNamed WithName(this StreetNameWasNamed e, string name)
        {
            return new StreetNameWasNamed(new StreetNameId(e.StreetNameId), new StreetNameName(name, e.Language));
        }

        public static StreetNameWasNamed WithLanguage(this StreetNameWasNamed e, Language? language)
        {
            return new StreetNameWasNamed(new StreetNameId(e.StreetNameId), new StreetNameName(e.Name, language));
        }

        public static StreetNameWasNamed WithProvenance(this StreetNameWasNamed e, Provenance provenance)
        {
            var newEvent = new StreetNameWasNamed(new StreetNameId(e.StreetNameId), new StreetNameName(e.Name, e.Language));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNameNameWasCorrected WithName(this StreetNameNameWasCorrected e, string name)
        {
            return new StreetNameNameWasCorrected(new StreetNameId(e.StreetNameId), new StreetNameName(name, e.Language));
        }

        public static StreetNameNameWasCorrected WithLanguage(this StreetNameNameWasCorrected e, Language? language)
        {
            return new StreetNameNameWasCorrected(new StreetNameId(e.StreetNameId), new StreetNameName(e.Name, language));
        }

        public static StreetNameNameWasCorrected WithProvenance(this StreetNameNameWasCorrected e, Provenance provenance)
        {
            var newEvent = new StreetNameNameWasCorrected(new StreetNameId(e.StreetNameId), new StreetNameName(e.Name, e.Language));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNameNameWasCleared WithId(this StreetNameNameWasCleared e, Guid id)
        {
            return new StreetNameNameWasCleared(new StreetNameId(id), e.Language);
        }

        public static StreetNameNameWasCleared WithLanguage(this StreetNameNameWasCleared e, Language? language)
        {
            return new StreetNameNameWasCleared(new StreetNameId(e.StreetNameId), language);
        }

        public static StreetNameNameWasCleared WithProvenance(this StreetNameNameWasCleared e, Provenance provenance)
        {
            var newEvent = new StreetNameNameWasCleared(new StreetNameId(e.StreetNameId), e.Language);
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }
public static StreetNameNameWasCorrectedToCleared WithId(this StreetNameNameWasCorrectedToCleared e, Guid id)
        {
            return new StreetNameNameWasCorrectedToCleared(new StreetNameId(id), e.Language);
        }

        public static StreetNameNameWasCorrectedToCleared WithLanguage(this StreetNameNameWasCorrectedToCleared e, Language? language)
        {
            return new StreetNameNameWasCorrectedToCleared(new StreetNameId(e.StreetNameId), language);
        }

        public static StreetNameNameWasCorrectedToCleared WithProvenance(this StreetNameNameWasCorrectedToCleared e, Provenance provenance)
        {
            var newEvent = new StreetNameNameWasCorrectedToCleared(new StreetNameId(e.StreetNameId), e.Language);
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNameHomonymAdditionWasDefined WithHomonymAddition(this StreetNameHomonymAdditionWasDefined e, StreetNameHomonymAddition homonymAddition, Provenance provenance)
        {
            var newEvent =  new StreetNameHomonymAdditionWasDefined(new StreetNameId(e.StreetNameId), homonymAddition);
            ((ISetProvenance)newEvent).SetProvenance(provenance);
            return newEvent;
        }

        public static StreetNameHomonymAdditionWasCleared WithLanguage(this StreetNameHomonymAdditionWasCleared e, Language language, Provenance provenance)
        {
            var newEvent =  new StreetNameHomonymAdditionWasCleared(new StreetNameId(e.StreetNameId), language);
            ((ISetProvenance)newEvent).SetProvenance(provenance);
            return newEvent;
        }

        public static StreetNameHomonymAdditionWasCorrectedToCleared WithLanguage(this StreetNameHomonymAdditionWasCorrectedToCleared e, Language language, Provenance provenance)
        {
            var newEvent =  new StreetNameHomonymAdditionWasCorrectedToCleared(new StreetNameId(e.StreetNameId), language);
            ((ISetProvenance)newEvent).SetProvenance(provenance);
            return newEvent;
        }

        public static StreetNameHomonymAdditionWasCorrected WithHomonymAddition(this StreetNameHomonymAdditionWasCorrected e, StreetNameHomonymAddition homonymAddition, Provenance provenance)
        {
            var newEvent =  new StreetNameHomonymAdditionWasCorrected(new StreetNameId(e.StreetNameId), homonymAddition);
            ((ISetProvenance)newEvent).SetProvenance(provenance);
            return newEvent;
        }

        public static StreetNameBecameComplete WithId(this StreetNameBecameComplete e, Guid id)
        {
            return new StreetNameBecameComplete(new StreetNameId(id));
        }

        public static StreetNameBecameComplete WithProvenance(this StreetNameBecameComplete e, Provenance provenance)
        {
            var newEvent = new StreetNameBecameComplete(new StreetNameId(e.StreetNameId));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNameWasRemoved WithId(this StreetNameWasRemoved e, Guid id)
        {
            return new StreetNameWasRemoved(new StreetNameId(id));
        }

        public static StreetNameWasRemoved WithProvenance(this StreetNameWasRemoved e, Provenance provenance)
        {
            var newEvent = new StreetNameWasRemoved(new StreetNameId(e.StreetNameId));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static StreetNameWasRetired WithId(this StreetNameWasRetired e, Guid id)
        {
            return new StreetNameWasRetired(new StreetNameId(id));
        }

        public static StreetNameWasRetired WithProvenance(this StreetNameWasRetired e, Provenance provenance)
        {
            var newEvent = new StreetNameWasRetired(new StreetNameId(e.StreetNameId));
            ((ISetProvenance)newEvent).SetProvenance(provenance);

            return newEvent;
        }

        public static AtomEntry WithObjectId<TContent>(this AtomEntry entry, Guid id)
        {
            var syndicationContent = (SyndicationContent<TContent>)entry.Content;
            if (!(syndicationContent.Object is TContent content))
                return entry;

            var idProperty = typeof(TContent).GetProperty("Id", typeof(Guid));
            if (idProperty == null)
                return entry;

            idProperty.SetValue(content, id);

            return entry;
        }
    }
}
