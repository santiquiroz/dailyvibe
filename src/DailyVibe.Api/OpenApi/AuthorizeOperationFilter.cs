using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DailyVibe.Api.OpenApi;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!RequiresAuthorization(context.ApiDescription.ActionDescriptor.EndpointMetadata))
        {
            return;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SwaggerServiceExtensions.BearerSchemeId, context.Document)] = [],
        });
    }

    private static bool RequiresAuthorization(IList<object> endpointMetadata) =>
        endpointMetadata.OfType<IAuthorizeData>().Any() && !endpointMetadata.OfType<IAllowAnonymous>().Any();
}
