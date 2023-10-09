namespace StreetNameRegistry.Consumer.Read.Postal.Projections
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.PostalRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public class PostalKafkaProjection : ConnectedProjection<ConsumerPostalContext>
    {
        public PostalKafkaProjection()
        {
            When<PostalInformationWasRegistered>(async (context, message, ct) =>
            {
                await context.PostalConsumerItems.AddAsync(new PostalConsumerItem(message.PostalCode), ct);
            });

            When<PostalInformationWasRealized>(async (context, message, ct) =>
            {
                await context.FindAndUpdate(message.PostalCode, i => i.Status = PostalStatus.Realized, ct);
            });

            When<PostalInformationWasRetired>(async (context, message, ct) =>
            {
                await context.FindAndUpdate(message.PostalCode, i => i.Status = PostalStatus.Retired, ct);
            });

            When<MunicipalityWasAttached>(async (context, message, ct) =>
            {
                await context.FindAndUpdate(message.PostalCode, i => i.NisCode = message.NisCode, ct);
            });
        }
    }
}
