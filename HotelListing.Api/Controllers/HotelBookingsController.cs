using HotelListing.Api.Contracts;
using HotelListing.Api.Models.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    // GET: api/hotels/10/bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetHotelBookings([FromRoute] int hotelId)
    {
        var result = await bookingService.GetHotelBookingsAsync(hotelId);

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
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    // PUT: api/hotels/10/bookings/5/admin/confirm
    // This endpoint allows an admin to confirm any booking
    [HttpPut("{bookingId:int}/admin/confirm")]
    public async Task<IActionResult> AdminConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }
}
