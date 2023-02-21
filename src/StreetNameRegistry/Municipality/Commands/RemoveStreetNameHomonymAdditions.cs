namespace StreetNameRegistry.Municipality.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Utilities;

public sealed class RemoveStreetNameHomonymAdditions : IHasCommandProvenance
{
    private static readonly Guid Namespace = new Guid("54c25aa4-18db-41d0-b13a-b4066620f62d");

    public MunicipalityId MunicipalityId { get; }
    public PersistentLocalId PersistentLocalId { get; }
    public List<Language> Languages { get; }
    public Provenance Provenance { get; }

    public RemoveStreetNameHomonymAdditions(
        MunicipalityId municipalityId,
        PersistentLocalId persistentLocalId,
        IEnumerable<Language> languages,
        Provenance provenance)
    {
        MunicipalityId = municipalityId;
        PersistentLocalId = persistentLocalId;
        Languages = languages.ToList();
        Provenance = provenance;
    }

    public Guid CreateCommandId()
        => Deterministic.Create(Namespace, $"{nameof(RemoveStreetNameHomonymAdditions)}-{ToString()}");

    public override string? ToString()
        => ToStringBuilder.ToString(IdentityFields());

    private IEnumerable<object> IdentityFields()
    {
        yield return MunicipalityId;
        yield return PersistentLocalId;

        foreach (var item in Languages)
        {
            yield return item;
        }

        foreach (var field in Provenance.GetIdentityFields())
        {
            yield return field;
        }
    }
}
