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

            When<PostalInformationPostalNameWasAdded>(async (context, message, ct) =>
            {
                switch (message.Language)
                {
                    case "Dutch": await context.FindAndUpdate(message.PostalCode, i => i.NameDutch = message.Name, ct);
                        break;
                    case "French": await context.FindAndUpdate(message.PostalCode, i => i.NameFrench = message.Name, ct);
                        break;
                    case "German": await context.FindAndUpdate(message.PostalCode, i => i.NameGerman = message.Name, ct);
                        break;
                    case "English": await context.FindAndUpdate(message.PostalCode, i => i.NameEnglish = message.Name, ct);
                        break;
                }
            });

            When<PostalInformationPostalNameWasRemoved>(async (context, message, ct) =>
            {
                switch (message.Language)
                {
                    case "Dutch": await context.FindAndUpdate(message.PostalCode, i => i.NameDutch = null, ct);
                        break;
                    case "French": await context.FindAndUpdate(message.PostalCode, i => i.NameFrench = null, ct);
                        break;
                    case "German": await context.FindAndUpdate(message.PostalCode, i => i.NameGerman = null, ct);
                        break;
                    case "English": await context.FindAndUpdate(message.PostalCode, i => i.NameEnglish = null, ct);
                        break;
                }
            });

            When<MunicipalityWasAttached>(async (context, message, ct) =>
            {
                await context.FindAndUpdate(message.PostalCode, i => i.NisCode = message.NisCode, ct);
            });
        }
    }
}
