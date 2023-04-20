namespace StreetNameRegistry.Tests.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance.AcmIdm;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Moq;
    using Municipality;
    using StreetNameRegistry.Api.BackOffice;
    using StreetNameRegistry.Api.BackOffice.Infrastructure;
    using StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization;
    using StreetNameRegistry.Api.BackOffice.Infrastructure.Options;
    using Testing;
    using Xunit.Abstractions;

    public abstract class BackOfficeApiTest<TController> : StreetNameRegistryTest
        where TController: BackOfficeApiController
    {
        protected readonly TController Controller;
        protected const string PublicTicketUrl = "https://www.ticketing.com";
        protected const string InternalTicketUrl = "https://www.internalticketing.com";
        protected IOptions<TicketingOptions> TicketingOptions { get; }
        protected Mock<IMediator> MockMediator { get; }
        protected Mock<IActionContextAccessor> MockActionContext { get; set; }

        protected BackOfficeApiTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            TicketingOptions = Options.Create(Fixture.Create<TicketingOptions>());
            TicketingOptions.Value.PublicBaseUrl = PublicTicketUrl;
            TicketingOptions.Value.InternalBaseUrl = InternalTicketUrl;

            MockActionContext = new Mock<IActionContextAccessor>();
            MockActionContext.SetupProperty(x => x.ActionContext, new ActionContext{ HttpContext = new DefaultHttpContext()});

            MockMediator = new Mock<IMediator>();
            Controller = CreateApiBusControllerWithUser();
        }

        protected void MockMediatorResponse<TRequest, TResponse>(TResponse response)
            where TRequest : IRequest<TResponse>
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(response));
        }

        protected INisCodeAuthorizer<T> MockNisCodeAuthorizer<T>(bool authorized = true)
        {
            var mock = new Mock<INisCodeAuthorizer<T>>();
            mock
                .Setup(x =>
                    x.IsNotAuthorized(It.IsAny<HttpContext>(), It.IsAny<T>(), CancellationToken.None))
                .ReturnsAsync(!authorized);
            return mock.Object;
        }

        protected IIfMatchHeaderValidator MockValidIfMatchValidator(bool result = true)
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();
            mockIfMatchHeaderValidator
                .Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<PersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(result));
            return mockIfMatchHeaderValidator.Object;
        }

        protected IValidator<TRequest> MockPassingRequestValidator<TRequest>()
        {
            var mockRequestValidator = new Mock<IValidator<TRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));
            return mockRequestValidator.Object;
        }

        protected string GetStreetNamePuri(int persistentLocalId)
            => $"https://data.vlaanderen.be/id/gemeente/{persistentLocalId}";

        protected Uri CreateTicketUri(Guid ticketId)
        {
            return new Uri($"{InternalTicketUrl}/tickets/{ticketId:D}");
        }

        public TController CreateApiBusControllerWithUser(string username = "John Doe")
        {
            var controller = Activator.CreateInstance(typeof(TController),
                MockMediator.Object,
                TicketingOptions,
                MockActionContext.Object,
                new AcmIdmProvenanceFactory(Application.StreetNameRegistry, MockActionContext.Object)) as TController;

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", username),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            if (controller != null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

                return controller;
            }

            throw new Exception("Could not find controller type");
        }

        protected void AssertLocation(string? location, Guid ticketId)
        {
            var expectedLocation = $"{PublicTicketUrl}/tickets/{ticketId:D}";

            location.Should().NotBeNullOrWhiteSpace();
            location.Should().Be(expectedLocation);
        }
    }
}
