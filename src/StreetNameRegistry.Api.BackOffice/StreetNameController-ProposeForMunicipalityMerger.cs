namespace StreetNameRegistry.Api.BackOffice
{
    using System;
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
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Consumer;
    using Consumer.Municipality;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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
        /// <param name="municipalityConsumerContext"></param>
        /// <param name="dryRun"></param>
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
            [FromServices] ConsumerContext municipalityConsumerContext,
            [FromQuery(Name = "dry-run")] bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a CSV file.");

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
                return BadRequest("Only CSV files are allowed.");

            try
            {
                var errorMessages = new List<string>();
                var records = new List<CsvRecord>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream, cancellationToken);
                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                           {
                               Delimiter = ";",
                               HasHeaderRecord = true,
                               IgnoreBlankLines = true
                           }))
                    {
                        await csv.ReadAsync();
                        csv.ReadHeader();

                        var recordNr = 0;
                        while (await csv.ReadAsync())
                        {
                            recordNr++;

                            var recordErrorMessages = new List<string>();

                            var oldNisCode = csv.GetField<string>("OUD NIS code");
                            var oldStreetNamePersistentLocalIdAsString = csv.GetField<string>("OUD straatnaamid");
                            var newNisCode = csv.GetField<string>("NIEUW NIS code");
                            var streetName = csv.GetField<string>("NIEUW straatnaam");
                            var homonymAddition = csv.GetField<string>("NIEUW homoniemtoevoeging");

                            if (string.IsNullOrWhiteSpace(oldNisCode))
                            {
                                recordErrorMessages.Add($"OldNisCode is required at record number {recordNr}");
                            }

                            if (string.IsNullOrWhiteSpace(oldStreetNamePersistentLocalIdAsString))
                            {
                                recordErrorMessages.Add($"OldStreetNamePersistentLocalId is required at record number {recordNr}");
                            }

                            if (!string.IsNullOrWhiteSpace(oldStreetNamePersistentLocalIdAsString)
                                & !int.TryParse(oldStreetNamePersistentLocalIdAsString, out var oldStreetNamePersistentLocalId))
                            {
                                recordErrorMessages.Add($"OldStreetNamePersistentLocalId is NaN at record number {recordNr}");
                            }

                            if (string.IsNullOrWhiteSpace(newNisCode))
                            {
                                recordErrorMessages.Add($"NisCode is required at record number {recordNr}");
                            }
                            else if (newNisCode.Trim() != nisCode)
                            {
                                recordErrorMessages.Add($"NisCode {newNisCode} does not match the provided NisCode {nisCode} at record number {recordNr}");
                            }

                            if (string.IsNullOrWhiteSpace(streetName))
                            {
                                recordErrorMessages.Add($"StreetName is required at record number {recordNr}");
                            }

                            if (recordErrorMessages.Any())
                            {
                                errorMessages.AddRange(recordErrorMessages);
                                continue;
                            }

                            records.Add(new CsvRecord
                            {
                                OldNisCode = oldNisCode!.Trim(),
                                OldStreetNamePersistentLocalId = oldStreetNamePersistentLocalId,
                                NisCode = nisCode.Trim(),
                                StreetName = streetName!.Trim(),
                                HomonymAddition = string.IsNullOrWhiteSpace(homonymAddition) ? null : homonymAddition.Trim()
                            });
                        }
                    }
                }

                var oldMunicipalities = new List<MunicipalityConsumerItem>();
                foreach (var oldMunicipalityNisCode in records.Select(x => x.OldNisCode).Distinct())
                {
                    var oldMunicipality = await municipalityConsumerContext.MunicipalityConsumerItems.SingleOrDefaultAsync(
                        x => x.NisCode == oldMunicipalityNisCode, cancellationToken: cancellationToken);

                    if (oldMunicipality is null)
                    {
                        errorMessages.Add($"No municipality found for NisCode '{oldMunicipalityNisCode}'");
                        continue;
                    }

                    oldMunicipalities.Add(oldMunicipality);
                }

                errorMessages.AddRange(records
                    .GroupBy(x => new
                    {
                        x.OldStreetNamePersistentLocalId,
                        StreetName = x.StreetName.ToLowerInvariant()
                    })
                    .Where(x => x.Count() > 1)
                    .Select(x =>
                        $"Duplicate record for streetName with persistent local id {x.Key.OldStreetNamePersistentLocalId}"));

                if (errorMessages.Any())
                {
                    return BadRequest(errorMessages);
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
                        y => y.Select(z => new MergedStreetName(
                            z.OldStreetNamePersistentLocalId,
                            oldMunicipalities.Single(x => x.NisCode == z.OldNisCode).MunicipalityId)).ToList());



                //if merged streetnames are more than one for a streetname, then it's a merger (combination) of the streetnames
                var combinedStreetNames = streetNamesByNisCode.Where(x => x.Value.Count > 1).ToList();
                foreach (var combinedStreetName in combinedStreetNames)
                {
                    var numberOfSplits = 0;
                    foreach (var mergedStreetName in combinedStreetName.Value)
                    {
                        var count = streetNamesByNisCode.Count(x =>
                            x.Value.Any(y => y.StreetNamePersistentLocalId == mergedStreetName.StreetNamePersistentLocalId));
                        if (count > 1)
                            numberOfSplits++;
                    }

                    //we more than one split occurs with merger, then it's impossible to deduce which streetname was replaced by which
                    if(numberOfSplits > 1)
                        errorMessages.Add($"Trying to combine and split streetname '{combinedStreetName.Key.StreetName}' is not supported");
                }

                if (errorMessages.Any())
                {
                    return BadRequest(errorMessages.Distinct());
                }

                if (dryRun)
                    return NoContent();

                var result = await _mediator
                    .Send(
                        new ProposeStreetNamesForMunicipalityMergerSqsRequest(
                            nisCode,
                            streetNamesByNisCode.Select(x => new ProposeStreetNamesForMunicipalityMergerSqsRequestItem(
                                persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                                x.Key.StreetName,
                                x.Key.HomonymAddition,
                                x.Value)).ToList(),
                            new ProvenanceData(CreateProvenance(Modification.Insert, $"Fusie {nisCode}")))
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
        public required string OldNisCode { get; init; }
        public required int OldStreetNamePersistentLocalId { get; init; }
        public required string NisCode { get; init; }
        public required string StreetName { get; init; }
        public string? HomonymAddition { get; init; }
    }
}
