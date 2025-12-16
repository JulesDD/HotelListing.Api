using HotelListing.Api.Models.Booking;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts;

public interface IBookingService
{
    Task<Result> AdminCancelBookingsAsync(int hotelId, int bookingId);
    Task<Result> AdminConfirmBookingsAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingsAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingsAsync(int hotelId, int bookingId);
    Task<Result<GetBookingDto>> CreateBookingsAsync(CreateBookingDto createBookingDto);
    Task<Result<IEnumerable<GetBookingDto>>> GetHotelBookingsAsync(int hotelId);
    Task<Result<GetBookingDto>> UpdateBookingsAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
}