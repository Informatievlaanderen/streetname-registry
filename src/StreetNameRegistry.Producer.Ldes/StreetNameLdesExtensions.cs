namespace StreetNameRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Municipality;

    public static class StreetNameLdesExtensions
    {
        public static async Task FindAndUpdateStreetNameDetail(
            this ProducerContext context,
            int persistentLocalId,
            Action<StreetNameDetail> updateFunc,
            CancellationToken ct)
        {
            var streetName = await context
                .StreetNames
                .FindAsync(persistentLocalId, cancellationToken: ct);

            if (streetName is null)
            {
                throw new ProjectionItemNotFoundException<ProducerProjections>(persistentLocalId.ToString());
            }

            updateFunc(streetName);
        }

        public static StraatnaamStatus ConvertToStraatnaamStatus(this StreetNameStatus status)
        {
            switch (status)
            {
                case StreetNameStatus.Proposed:
                    return StraatnaamStatus.Voorgesteld;
                case StreetNameStatus.Current:
                    return StraatnaamStatus.InGebruik;
                case StreetNameStatus.Retired:
                    return StraatnaamStatus.Gehistoreerd;
                case StreetNameStatus.Rejected:
                    return StraatnaamStatus.Afgekeurd;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
