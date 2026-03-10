using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Booking;
using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

/// <summary>
/// Handles hotel booking operations including retrieval, creation, updating, cancellation, and administrative actions
/// for hotel bookings.
/// </summary>
/// <param name="bookingService">Service for managing booking-related operations.</param>
[Route("api/hotels/v{version:apiVersion}/{hotelId:int}/bookings")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    /// <summary>
    /// Retrieves a paged list of bookings for a specified hotel, with optional filtering and pagination.
    /// </summary>
    /// <remarks>Access to this endpoint is restricted to admin users.</remarks>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="paginationParameters">Pagination settings for the result set.</param>
    /// <param name="bookingFilteringParameters">Filtering options to apply to the bookings.</param>
    /// <returns>A paged result containing booking data for the specified hotel.</returns>
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

    /// <summary>
    /// Creates a new booking for the specified hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel for which the booking is created.</param>
    /// <param name="createBookingDto">The booking details.</param>
    /// <returns>An ActionResult containing the created booking information.</returns>
    // POST: api/hotels/10/bookings
    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute]int hotelId, [FromBody] CreateBookingDto createBookingDto)
     {
        var result = await bookingService.CreateBookingsAsync(createBookingDto);
        return ToActionResult(result);
    }

    /// <summary>
    /// Updates an existing booking for a specified hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel containing the booking.</param>
    /// <param name="bookingId">The ID of the booking to update.</param>
    /// <param name="updateBookingDto">The updated booking information.</param>
    /// <returns>An ActionResult containing the updated booking details.</returns>
    // PUT: api/hotels/10/bookings/5
    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking([FromRoute] int hotelId, [FromRoute] int bookingId, [FromBody] UpdateBookingDto updateBookingDto)
    {
        var result = await bookingService.UpdateBookingsAsync(hotelId,bookingId, updateBookingDto);
        return ToActionResult(result);
    }
    
    /// <summary>
    /// Cancels a specific booking for a hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <param name="bookingId">The ID of the booking to cancel.</param>
    /// <returns>An IActionResult indicating the result of the cancellation operation.</returns>
    // PUT: api/hotels/10/bookings/5/cancel
    [HttpPut("{bookingId:int}/cancel")]
    public async Task<IActionResult> CancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.CancelBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels a booking for the specified hotel as an administrator.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <param name="bookingId">The ID of the booking to cancel.</param>
    /// <returns>An IActionResult indicating the outcome of the cancellation.</returns>
    // PUT: api/hotels/10/bookings/5/admin/cancel
    // This endpoint allows an admin to cancel any booking
    [HttpPut("{bookingId:int}/admin/cancel")]
    [AdminAttributes]
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingsAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    /// <summary>
    /// Allows an admin to confirm a booking for a specified hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <param name="bookingId">The ID of the booking to confirm.</param>
    /// <returns>An IActionResult indicating the result of the confirmation operation.</returns>
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
