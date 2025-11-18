using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController(IHotelsServices hotelsServices) : ControllerBase
{

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelDto>>> GetHotels()
    {
        var hotels = await hotelsServices.GetHotelsAsync();
        return Ok(hotels);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotel = await hotelsServices.GetHotelAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest("Invalid Hotel Id");
        }

        try
        {             
            await hotelsServices.UpdateHotelAsync(id, updateDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await hotelsServices.HotelExistsAsync(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        await hotelsServices.UpdateHotelAsync(id, updateDto);

        return NoContent();
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDto createDto)
    {
        var hotel = await hotelsServices.CreateHotelAsync(createDto);

        return CreatedAtAction("GetHotel", new { name = hotel.Name }, hotel);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        await hotelsServices.DeleteHotelAsync(id);
  
        return NoContent();
    }
}