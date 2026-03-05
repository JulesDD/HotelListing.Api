using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Booking;
using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels/v{version:apiVersion}/{hotelId:int}/bookings")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    // GET: api/hotels/10/bookings
    // This endpoint is restricted to admin users only using the AdminAttributes filter
    [HttpGet]
    [AdminAttributes]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetHotelBookings([FromRoute] int hotelId, [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] BookingFilteringParameters bookingFilteringParameters)
    {
        var result = await bookingService.GetHotelBookingsAsync(hotelId, paginationParameters, bookingFilteringParameters);

        return ToActionResult(result);
    }

    // POST: api/hotels/10/bookings
    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute]int hotelId, [FromBody] CreateBookingDto createBookingDto)
     {
        var result = await bookingService.CreateBookingsAsync(createBookingDto);
        return ToActionResult(result);
    }

    // PUT: api/hotels/10/bookings/5
    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking([FromRoute] int hotelId, [FromRoute] int bookingId, [FromBody] UpdateBookingDto updateBookingDto)
    {
        var result = await bookingService.UpdateBookingsAsync(hotelId,bookingId, updateBookingDto);
        return ToActionResult(result);
    }
    
    // PUT: api/hotels/10/bookings/5/cancel
    [HttpPut("{bookingId:int}/cancel")]
    public async Task<IActionResult> CancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.CancelBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    // PUT: api/hotels/10/bookings/5/admin/cancel
    // This endpoint allows an admin to cancel any booking
    [HttpPut("{bookingId:int}/admin/cancel")]
    [AdminAttributes]
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    // PUT: api/hotels/10/bookings/5/admin/confirm
    // This endpoint allows an admin to confirm any booking
    [HttpPut("{bookingId:int}/admin/confirm")]
    [AdminAttributes]
    public async Task<IActionResult> AdminConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }
}
