using HotelListing.Api.Contracts;
using HotelListing.Api.Models.Hotel;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HotelsController(IHotelsServices hotelsService) : BaseApiController
{
    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelDto>>> GetHotels()
    {
        var result = await hotelsService.GetHotelsAsync();
        return ToActionResult(result);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var result = await hotelsService.GetHotelAsync(id);
        return ToActionResult(result);
    }

    // PUT: api/Hotels/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest("Id route value must match payload Id.");
        }

        var result = await hotelsService.UpdateHotelAsync(id, hotelDto);
        return ToActionResult(result);
    }

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

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await hotelsService.DeleteHotelAsync(id);
        return ToActionResult(result);
    }
}