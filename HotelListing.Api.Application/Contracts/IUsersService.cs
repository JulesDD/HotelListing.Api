using HotelListing.Api.Common.Result;
using HotelListing.Api.Application.Models.Auth;

namespace HotelListing.Api.Application.Contracts
{
    public interface IUsersService
    {
        string GetUserId { get; }

        Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}