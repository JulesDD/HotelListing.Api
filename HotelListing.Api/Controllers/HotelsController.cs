using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Hotel;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

/// <summary>
/// API controller for managing hotel resources, supporting operations such as retrieval, creation, updating, and
/// deletion of hotels.
/// </summary>
/// <param name="hotelsService">Service for handling hotel-related business logic.</param>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class HotelsController(IHotelsServices hotelsService) : BaseApiController
{
    /// <summary>
    /// Retrieves a paged list of hotels with optional filtering.
    /// </summary>
    /// <param name="paginationParameters">Pagination settings for the result set.</param>
    /// <param name="hotelFilteringParameters">Filtering criteria for hotels.</param>
    /// <returns>A paged result containing hotel data.</returns>
    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetHotelDto>>> GetHotels([FromQuery] PaginationParameters paginationParameters, 
        [FromQuery] HotelFilteringParameters hotelFilteringParameters)
    {
        var result = await hotelsService.GetHotelsAsync(paginationParameters, hotelFilteringParameters);
        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves details of a hotel by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel to retrieve.</param>
    /// <returns>An ActionResult containing the hotel details if found; otherwise, a not found result.</returns>
    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var result = await hotelsService.GetHotelAsync(id);
        return ToActionResult(result);
    }

    /// <summary>
    /// Updates an existing hotel with the specified ID using the provided data.
    /// </summary>
    /// <param name="id">The ID of the hotel to update.</param>
    /// <param name="hotelDto">The updated hotel data.</param>
    /// <returns>An IActionResult indicating the outcome of the update operation.</returns>
    // PUT: api/Hotels/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest("Id route value must match payload Id.");
        }

        var result = await hotelsService.UpdateHotelAsync(id, hotelDto);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new hotel and returns the created hotel details.
    /// </summary>
    /// <remarks>Only users with the Administrator role are authorized to perform this action.</remarks>
    /// <param name="hotelDto">The data for the hotel to create.</param>
    /// <returns>The created hotel details with a 201 status code, or an error response if creation fails.</returns>
    // POST: api/Hotels
    //Allow only administrators to create new hotels. This is enforced using the [Authorize] attribute with the Roles parameter set to "Administrator".
    //Also testing the creation of a new hotel should be done with an authenticated user who has the Administrator role.
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var result = await hotelsService.CreateHotelAsync(hotelDto);
        if (!result.IsSuccess) return MapErrorsToResponse(result.Errors);

        return CreatedAtAction(nameof(GetHotel), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Deletes a hotel with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the hotel to delete.</param>
    /// <returns>An IActionResult indicating the result of the delete operation.</returns>
    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await hotelsService.DeleteHotelAsync(id);
        return ToActionResult(result);
    }
}