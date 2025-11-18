using HotelListing.Api.Models.Hotel;

namespace HotelListing.Api.Contracts
{
    public interface IHotelsServices
    {
        Task<GetHotelDto> CreateHotelAsync(CreateHotelDto createDto);
        Task DeleteHotelAsync(int id);
        Task<GetHotelDto?> GetHotelAsync(int id);
        Task<IEnumerable<GetHotelDto>> GetHotelsAsync();
        Task<bool> HotelExistsAsync(int id);
        Task<bool> HotelExistsAsync(string name);
        Task UpdateHotelAsync(int id, UpdateHotelDto updateDto);
    }
}