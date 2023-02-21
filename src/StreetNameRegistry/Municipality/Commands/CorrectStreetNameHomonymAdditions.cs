namespace StreetNameRegistry.Municipality.Commands;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Utilities;

public sealed class CorrectStreetNameHomonymAdditions : IHasCommandProvenance
{
    private static readonly Guid Namespace = new Guid("5c1f8ae1-04c6-4119-8e30-d5d8d836fd36");

    public MunicipalityId MunicipalityId { get; }
    public PersistentLocalId PersistentLocalId { get; }
    public HomonymAdditions HomonymAdditions { get; }
    public Provenance Provenance { get; }

    public CorrectStreetNameHomonymAdditions(
        MunicipalityId municipalityId,
        PersistentLocalId persistentLocalId,
        HomonymAdditions homonymAdditions,
        Provenance provenance)
    {
        MunicipalityId = municipalityId;
        PersistentLocalId = persistentLocalId;
        HomonymAdditions = homonymAdditions;
        Provenance = provenance;
    }

    public Guid CreateCommandId()
        => Deterministic.Create(Namespace, $"{nameof(CorrectStreetNameHomonymAdditions)}-{ToString()}");

    public override string? ToString()
        => ToStringBuilder.ToString(IdentityFields());

    private IEnumerable<object> IdentityFields()
    {
        yield return MunicipalityId;
        yield return PersistentLocalId;

        foreach (var item in HomonymAdditions)
        {
            yield return item;
        }

        foreach (var field in Provenance.GetIdentityFields())
        {
            yield return field;
        }
    }
}
