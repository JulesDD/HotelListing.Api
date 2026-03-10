using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//Created a controller for authentication
namespace HotelListing.Api.Controllers;

/// <summary>
/// Provides authentication endpoints for user registration and login.
/// </summary>
/// <param name="usersService">Service for handling user-related authentication operations.</param>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
[AllowAnonymous]
public class AuthController(IUsersService usersService) : BaseApiController
{
    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="registerUserDto">The registration information for the new user.</param>
    /// <returns>An ActionResult containing the registered user's data.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await usersService.RegisterAsync(registerUserDto);
        return ToActionResult(result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="loginUserDto">The login credentials of the user.</param>
    /// <returns>A JWT token as a string if authentication is successful; otherwise, an error response.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
        var result = await usersService.LoginAsync(loginUserDto);
        return ToActionResult(result);
    }
}
