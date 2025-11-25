using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Auth;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Services;

public class UsersService(UserManager<ApplicationUser> userManager) : IUsersService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        // Check if the user already exists
        var user = new ApplicationUser
        {
            UserName = registerUserDto.Email,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName
        };

        // Create the user
        var result = await userManager.CreateAsync(user, registerUserDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(ErrorCodes.BadRequest, e.Description)).ToArray();
            return Result<RegisteredUserDto>.BadRequest(errors);
        }

        var registeredUserDto = new RegisteredUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };


        return Result<RegisteredUserDto>.Success(registeredUserDto);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        // Check if the user exists
        var user = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (user is null)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Email not found!"));
        }
        // Check if the password is correct
        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
        if (!isPasswordValid)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid password!"));
        }
        return Result<string>.Success("Login Successful!");
    }
}
