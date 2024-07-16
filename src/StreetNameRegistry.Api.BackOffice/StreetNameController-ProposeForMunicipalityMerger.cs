namespace StreetNameRegistry.Api.BackOffice
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Municipality;
    using Swashbuckle.AspNetCore.Filters;

    public partial class StreetNameController
    {
        /// <summary>
        /// Stel een straatnaam voor.
        /// Accept a csv file with street names and their municipality codes.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nisCode"></param>
        /// <param name="persistentLocalIdGenerator"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("acties/voorstellen/gemeentefusie/{niscode}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
        public async Task<IActionResult> ProposeForMunicipalityMerger(
            IFormFile? file,
            [FromRoute(Name = "niscode")] string nisCode,
            [FromServices] IPersistentLocalIdGenerator persistentLocalIdGenerator,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a CSV file.");

            if (!Path.GetExtension(file.FileName).Equals(".csv", System.StringComparison.CurrentCultureIgnoreCase))
                return BadRequest("Only CSV files are allowed.");

            try
            {
                var records = new List<CsvRecord>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream, cancellationToken);
                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                           {
                               Delimiter = ";"
                           }))
                    {
                        var recordNr = 0;
                        while (await csv.ReadAsync())
                        {
                            recordNr++;

                            var oldStreetNamePersistentLocalId = csv.GetField<string>(0);
                            var newNisCode = csv.GetField<string>(1);
                            var streetName = csv.GetField<string>(2);

                            if(string.IsNullOrWhiteSpace(oldStreetNamePersistentLocalId))
                                return BadRequest($"OldStreetNamePersistentLocalId is required at record number {recordNr}");

                            if (string.IsNullOrWhiteSpace(newNisCode))
                                return BadRequest($"NisCode is required at record number {recordNr}");

                            if (newNisCode.Trim() != nisCode)
                                return BadRequest(
                                    $"NisCode {newNisCode} does not match the provided NisCode {nisCode} at record number {recordNr}");

                            if (string.IsNullOrWhiteSpace(streetName))
                                return BadRequest($"StreetName is required at record number {recordNr}");

                            records.Add(new CsvRecord
                            {
                                OldStreetNamePersistentLocalId = oldStreetNamePersistentLocalId.Trim(),
                                NisCode = nisCode.Trim(),
                                StreetName = streetName.Trim(),
                                HomonymAddition = csv.GetField<string>(3)?.Trim()
                            });
                        }
                    }
                }

                var streetNamesByNisCodeRecords = records
                    .GroupBy(x => x.NisCode)
                    .ToDictionary(
                        x => x.Key,
                        y => y.ToList())
                    .Single()
                    .Value;

                // group by streetname and homonym addition
                var streetNamesByNisCode = streetNamesByNisCodeRecords
                    .GroupBy(x => (x.StreetName, x.HomonymAddition))
                    .ToDictionary(
                        x => x.Key,
                        y => y
                            .Select(z =>  z.OldStreetNamePersistentLocalId.AsIdentifier().Map(int.Parse).Value).ToList());

                var result = await _mediator
                    .Send(
                        new ProposeStreetNamesForMunicipalityMergerSqsRequest(
                            nisCode,
                            streetNamesByNisCode.Select(x => new ProposeStreetNamesForMunicipalityMergerSqsRequestItem(
                                persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                                x.Key.StreetName,
                                x.Key.HomonymAddition,
                                x.Value)).ToList(),
                            new ProvenanceData(CreateProvenance(Modification.Insert)))
                        , cancellationToken);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrors.ProposeStreetName.MunicipalityUnknown.Code,
                    string.Empty,
                    ValidationErrors.ProposeStreetName.MunicipalityUnknown.Message(""));
            }
        }
    }

    public sealed class CsvRecord
    {
        public required string OldStreetNamePersistentLocalId { get; init; }
        public required string NisCode { get; init; }
        public required string StreetName { get; init; }
        public string? HomonymAddition { get; init; }
    }
}