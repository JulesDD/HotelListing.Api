using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelListing.Api.Filters;

public class SecurityRequirementsOperationFilters : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (hasAuthorize?.Any()==true)
        {
            // Add the security requirement for the API key authentication scheme
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var securityRequirement = new List<OpenApiSecurityRequirement>();

            // Check for API key authentication scheme
            if (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name.Contains("ApiKey")) == true)
            {
                {
                    securityRequirement.Add(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "ApiKey"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                }
            }

            // Check for Basic authentication scheme
            if (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name.Contains("Basic")) == true)
                {
                    {
                        securityRequirement.Add(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Basic"
                                    }
                                },
                                Array.Empty<string>()
                            }
                        });
                    }
            }

            // If there are any security requirements, add them to the operation
            if (securityRequirement.Any())
            {
                operation.Security = securityRequirement;
            }
        }
    }
}
