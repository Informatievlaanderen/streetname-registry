namespace StreetNameRegistry.Tests.BackOffice.Api.WhenCorrectingStreetNameHomonymAdditions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Abstractions.Requests;
    using StreetNameRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Municipality;
    using Xunit;
    using Xunit.Abstractions;
    using FluentValidation;
    using Municipality.Exceptions;
    using StreetNameRegistry.Api.BackOffice.Validators;

    public sealed class GivenHomonymAdditionExceedsMaxLength : BackOfficeApiTest<StreetNameController>
    {
        public GivenHomonymAdditionExceedsMaxLength(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsHomonymAdditionMaxCharacterLengthExceededException()
        {
            var homonymAddition = "this homonymaddition exceeds max length of 20";
            Func<Task> act = async () =>
            {
                await Controller.CorrectHomonymAdditions(
                    MockValidIfMatchValidator(),
                    new StreetNameCorrectHomonymAdditionsRequestValidator(),
                    123,
                    new CorrectStreetNameHomonymAdditionsRequest
                    {
                        HomoniemToevoegingen = new Dictionary<Taal, string>
                        {
                            {Taal.NL, homonymAddition}
                        }
                    },
                    string.Empty,
                    CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(x =>
                    x.ErrorMessage == $"Maximum lengte van een homoniemToevoeging in 'nl' is 20 tekens. U heeft momenteel {homonymAddition.Length} tekens." &&
                    x.ErrorCode == "StraatnaamHomoniemToevoegingMaxlengteValidatie"));
        }
    }
}
