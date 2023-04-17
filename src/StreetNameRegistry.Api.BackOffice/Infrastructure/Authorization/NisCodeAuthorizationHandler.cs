namespace StreetNameRegistry.Api.BackOffice.Infrastructure.Authorization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Metadata;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NisCodeService.Abstractions;

    public class NisCodeAuthorizationHandler : AuthorizationHandler<NisCodeAuthorizationRequirement>
    {
        private readonly INisCodeService _nisCodeService;
        private readonly BackOfficeContext _dbContext;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public NisCodeAuthorizationHandler(
            INisCodeService nisCodeService,
            BackOfficeContext dbContext,
            JsonSerializerSettings? jsonSerializerSettings = null)
        {
            _nisCodeService = nisCodeService;
            _dbContext = dbContext;
            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NisCodeAuthorizationRequirement requirement)
        {
            var claim = context.User.FindFirst(AcmIdmClaimTypes.VoOrgCode);

            if (claim == null)
            {
                context.Fail();
                return;
            }

            var streetNameNisCodeFinderService = new StreetNameNisCodeFinderService(context, _dbContext, requirement, _jsonSerializerSettings);
            var streetNameNisCode = await streetNameNisCodeFinderService.Find();

            var ovoNisCode = await _nisCodeService.Get(/*claim.Value*/"001999");

            if (!string.IsNullOrEmpty(ovoNisCode) && ovoNisCode == streetNameNisCode)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }

    public class NisCodeAuthorizationRequirement : IAuthorizationRequirement
    {
        public string RouteParameterName { get; }

        public NisCodeAuthorizationRequirement(string routeParameterName)
        {
            RouteParameterName = routeParameterName;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PersistentLocalIdAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider, IFromRouteMetadata
    {
        /// <inheritdoc />
        public BindingSource BindingSource => BindingSource.Path;

        /// <summary>
        /// The <see cref="HttpRequest.RouteValues"/> name.
        /// </summary>
        public string? Name { get; set; }
    }

    public class StreetNameNisCodeFinderService : IStreetNamePersistentLocalIdToNisCodeService
    {
        private readonly AuthorizationHandlerContext _context;
        private readonly BackOfficeContext _dbContext;
        private readonly NisCodeAuthorizationRequirement _requirement;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public StreetNameNisCodeFinderService(
            AuthorizationHandlerContext context,
            BackOfficeContext dbContext,
            NisCodeAuthorizationRequirement requirement,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _context = context;
            _dbContext = dbContext;
            _requirement = requirement;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public async Task<string?> Find(CancellationToken cancellationToken = default)
        {
            string? streetNameId = null;

            var authContext = _context.Resource as AuthorizationFilterContext;

            if (authContext is null)
            {
                return null;
            }

            var persistenLocalId = (authContext.ActionDescriptor as ControllerActionDescriptor)?
                .MethodInfo.GetCustomAttributes(typeof(PersistentLocalIdAttribute), true)
                .OfType<PersistentLocalIdAttribute>()
                .FirstOrDefault()?.Name;

            // Try to get the streetnameId from the route parameter
            if (authContext.HttpContext.Request.RouteValues.TryGetValue(_requirement.RouteParameterName, out var routeValue))
            {
                streetNameId =  routeValue as string;
            }

            if (streetNameId is null && authContext.HttpContext.Request is {Method: "POST", ContentType: "application/json"})
            {
                var requestType = authContext.ActionDescriptor.Parameters
                    .FirstOrDefault(x => x.ParameterType.GetInterfaces().Any(i => i == typeof(IHaveStreetNameId)))
                    ?.ParameterType;

                if (requestType is not null)
                {
                    // Read the request body as a stream
                    var requestBodyStream = authContext.HttpContext.Request.Body;

                    // Deserialize the request body to the implementation type of IHaveStreetNameId
                    var serializer = JsonSerializer.CreateDefault(_jsonSerializerSettings);
                    var request = (IHaveStreetNameId)serializer.Deserialize(new StreamReader(requestBodyStream), requestType);

                    // Get the streetnameId from the request object
                    streetNameId = request?.StreetNameId.ToString();

                    // Replace the request stream with a new stream containing the deserialized JSON,
                    // so that the controller action can still read the request body
                    requestBodyStream.Seek(0, SeekOrigin.Begin);
                    serializer.Serialize(new StreamWriter(requestBodyStream), request);
                    requestBodyStream.Seek(0, SeekOrigin.Begin);
                }
            }

            if (streetNameId is null || !int.TryParse(streetNameId, out var streetNamePersistentLocalId))
                return null;

            var streetNameRelation = await _dbContext.MunicipalityIdByPersistentLocalId
                .FirstOrDefaultAsync(s => s.PersistentLocalId == streetNamePersistentLocalId, cancellationToken);

            return streetNameRelation?.NisCode;
        }
    }

    public interface IStreetNamePersistentLocalIdToNisCodeService
    {
        Task<string?> Find(CancellationToken cancellationToken = default);
    }

    public interface IHaveStreetNameId
    {
        public int StreetNameId { get; }
    }
}
