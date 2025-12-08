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

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute]int hotelId, [FromBody] CreateBookingDto createBookingDto)
     {
        var result = await bookingService.CreateBookingsAsync(createBookingDto);
        return ToActionResult(result);
    }
}
