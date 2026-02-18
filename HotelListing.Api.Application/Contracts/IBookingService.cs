using HotelListing.Api.Application.Models.Booking;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Result;

namespace HotelListing.Api.Application.Contracts;

public interface IBookingService
{
    Task<Result> AdminCancelBookingsAsync(int hotelId, int bookingId);
    Task<Result> AdminConfirmBookingsAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingsAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingsAsync(int hotelId, int bookingId);
    Task<Result<GetBookingDto>> CreateBookingsAsync(CreateBookingDto createBookingDto);
    Task<Result<PagedResult<GetBookingDto>>> GetHotelBookingsAsync(int hotelId, PaginationParameters paginationParameters);
    Task<Result<GetBookingDto>> UpdateBookingsAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
}