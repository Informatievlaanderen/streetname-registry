namespace StreetNameRegistry.Consumer.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Municipality;
    using StreetNameRegistry.Municipality.Events;

    public class MunicipalityConsumerProjection : ConnectedProjection<ConsumerContext>
    {
        public MunicipalityConsumerProjection()
        {
            When<Envelope<MunicipalityWasImported>>(async (context, message, ct) =>
            {
                await context.MunicipalityConsumerItems.AddAsync(new MunicipalityConsumerItem
                {
                    MunicipalityId = message.Message.MunicipalityId,
                    NisCode = message.Message.NisCode
                }, ct);
            });

            When<Envelope<MunicipalityNisCodeWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.FindAsync<MunicipalityConsumerItem>(new object[] { message.Message.MunicipalityId }, ct);
                if (item == null)
                {
                    throw new InvalidOperationException($"MunicipalityConsumerItem with id {message.Message.MunicipalityId} could not be found.");
                }
                item.NisCode = message.Message.NisCode;
            });

            When<Envelope<MunicipalityWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.FindAsync<MunicipalityConsumerItem>(new object[] { message.Message.MunicipalityId }, ct);
                if (item == null)
                {
                    throw new InvalidOperationException($"MunicipalityConsumerItem with id {message.Message.MunicipalityId} could not be found.");
                }
                item.IsRemoved = true;
            });
        }
    }
}
