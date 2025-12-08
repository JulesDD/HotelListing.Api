using HotelListing.Api.Models.Booking;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts;

public interface IBookingService
{
    Task<Result<GetBookingDto>> CreateBookingsAsync(CreateBookingDto createBookingDto);
    Task<Result<IEnumerable<GetBookingDto>>> GetHotelBookingsAsync(int hotelId);
}