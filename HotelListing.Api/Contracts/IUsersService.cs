using HotelListing.Api.Models.Auth;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts
{
    public interface IUsersService
    {
        string GetUserId { get; }

        Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}