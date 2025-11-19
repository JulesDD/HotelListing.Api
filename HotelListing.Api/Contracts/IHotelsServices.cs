using HotelListing.Api.Models.Hotel;
using HotelListing.Api.Results;

namespace HotelListing.Api.Services
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