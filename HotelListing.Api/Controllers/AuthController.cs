using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Auth;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

//Created a controller for authentication
namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IUsersService usersService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await usersService.RegisterAsync(registerUserDto);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
        var result = await usersService.LoginAsync(loginUserDto);
        return ToActionResult(result);
    }
}
