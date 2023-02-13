namespace StreetNameRegistry.Api.BackOffice
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetNameRegistry.Infrastructure.Repositories;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("straatnamen")]
    [ApiExplorerSettings(GroupName = "Straatnamen")]
    public partial class StreetNameController : BackOfficeApiController
    {
        private readonly IMediator _mediator;
        private readonly StreetNames _streetNames;
        private readonly TicketingOptions _ticketingOptions;

        public StreetNameController(
            IMediator mediator,
            StreetNames streetNames,
            IOptions<TicketingOptions> ticketingOptions)
        {
            _mediator = mediator;
            _streetNames = streetNames;
            _ticketingOptions = ticketingOptions.Value;
        }

        private ValidationException CreateValidationException(string errorCode, string propertyName, string message)
        {
            var failure = new ValidationFailure(propertyName, message)
            {
                ErrorCode = errorCode
            };

            return new ValidationException(new List<ValidationFailure>
            {
                failure
            });
        }

        public IActionResult Accepted(LocationResult locationResult)
        {
            return Accepted(locationResult
                .Location
                .ToString()
                .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl));
        }
    }
}
