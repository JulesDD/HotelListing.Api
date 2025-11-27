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

public class BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUsersService usersService
    ): AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the Authorization header is present
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        // Get the Authorization header value
        var authHeader = authHeaderValues.ToString();
        if(string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }
        // Extract the token from the Authorization header
        var token = authHeader["Basic ".Length..].Trim();
        string decoded;
        
        // Decode the Base64 encoded token
        try
        {             
            var credentialBytes = Convert.FromBase64String(token);
            decoded = Encoding.UTF8.GetString(credentialBytes);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }

        // Split the decoded string into username and password
        var credentials = decoded.Split(':', 2);
        if (credentials.Length != 2)
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
        var usernameOrEmail = credentials[0];
        var password = credentials[1];

        // Validate the username and password
        var loginDto = new LoginUserDto
        {
            Email = usernameOrEmail,
            Password = password
        };

        var result = await usersService.LoginAsync(loginDto);
        if (!result.IsSuccess)
        {
            return AuthenticateResult.Fail("Invalid Username or Password");
        }

        // Create authenticated user principal
        // Claims represent the identity of the user
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, usernameOrEmail)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
