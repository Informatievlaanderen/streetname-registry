namespace StreetNameRegistry.Consumer.Read.Postal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Projections;

    public static class PostalConsumerExtensions
    {
        public static async Task<PostalConsumerItem> FindAndUpdate(
            this ConsumerPostalContext context,
            string postalCode,
            Action<PostalConsumerItem> updateFunc,
            CancellationToken ct)
        {
            var item = await context
                .PostalConsumerItems
                .FindAsync(new object?[] { postalCode }, ct);

            if (item == null)
                throw DatabaseItemNotFound(postalCode);

            updateFunc(item);

            return item;
        }

        private static ProjectionItemNotFoundException<PostalKafkaProjection> DatabaseItemNotFound(string postalCode)
            => new ProjectionItemNotFoundException<PostalKafkaProjection>(postalCode);
    }
}
