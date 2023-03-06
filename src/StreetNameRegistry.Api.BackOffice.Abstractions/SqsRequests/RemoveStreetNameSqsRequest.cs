namespace StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Requests;

public sealed class RemoveStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveStreetNameRequest>
{
    public RemoveStreetNameRequest Request { get; init; }
}
