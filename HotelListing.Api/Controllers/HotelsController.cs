using AutoMapper;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly IHotelsRepository _hotelsRepository;
    private readonly IMapper _mapper;

    public HotelsController(IHotelsRepository hotelsRepository, IMapper mapper)
    {
        this._hotelsRepository = hotelsRepository;
        this._mapper = mapper;
    }
    // GET: api/<HotelsController>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
    {
        var hotels = await _hotelsRepository.GetAllAsync();
        return Ok(_mapper.Map<List<HotelDto>>(hotels));
    }

    // GET api/<HotelsController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<HotelDto>> GetHotel(int id) 
    {
        //Search for Id in listing
        var hotel = await _hotelsRepository.GetAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<HotelDto>(hotel));
    }

    // POST api/<HotelsController>
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDto hotelDto)
    {
        var hotel = _mapper.Map<Hotel>(hotelDto);
        await _hotelsRepository.AddAsync(hotel);

        return CreatedAtAction("GetHotel", new { id = hotel.Id } ,hotel);
    }

    // PUT api/<HotelsController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult> PutHotel(int id, HotelDto hotelDto)
    {
        if (id != hotelDto.Id) 
        {
            return BadRequest();
        }

        var hotel = await _hotelsRepository.GetAsync(id);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        _mapper.Map(hotelDto, hotel);
        try
        {
            await _hotelsRepository.UpdateAsync(hotel);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await HotelExist(id)) 
            {
                return NotFound();
            }
        }
        return NoContent();
    }

    // DELETE api/<HotelsController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHotelDto(int id)
    {
        var hotel = await _hotelsRepository.GetAsync(id);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }
        await _hotelsRepository.DeleteAsync(id);
        return NoContent();

    }

    private async Task<bool> HotelExist(int id)
    {
        return await _hotelsRepository.Exists(id);
    }
}
