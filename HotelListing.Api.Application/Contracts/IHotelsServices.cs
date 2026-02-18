using HotelListing.Api.Common.Result;
using HotelListing.Api.Application.Models.Hotel;

namespace HotelListing.Api.Application.Contracts
{
    public interface IHotelsServices
    {
        Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto createDto);
        Task<Result> DeleteHotelAsync(int id);
        Task<Result<GetHotelDto?>> GetHotelAsync(int id);
        Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync();
        Task<bool> HotelExistsAsync(int id);
        Task<bool> HotelExistsAsync(string name, int countryId);
        Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto);
    }
}