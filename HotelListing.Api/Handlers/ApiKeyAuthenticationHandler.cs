using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.Api.Handlers;

public class ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyValidatorService apiKeyValidatorService
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
       string apiKey;

        // Check if the X-API-KEY header is present
        if (!Request.Headers.TryGetValue("X-API-KEY", out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }
        else
        {
            apiKey = apiKeyHeaderValues.ToString();
        }

        // Validate the API key
        var validApiKey = await apiKeyValidatorService.IsValidApiKeyAsync(apiKey, Context.RequestAborted);
        if(!validApiKey)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        // Create authenticated user principal
        // Claims represent the identity of the user
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier,"apikey"),
            new (ClaimTypes.Name, "API Key User")
        };



        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
