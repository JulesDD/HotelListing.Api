using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Country;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(ICountriesServices countriesServices) : ControllerBase
{
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
       var countries = await countriesServices.GetCountriesAsync();
        return Ok(countries);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountries(int id)
    {
        var country = await countriesServices.GetCountryAsync(id);

        if (country == null)
        {
            return NotFound();
        }

        return Ok(country);
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateDto)
    {
        if (id != updateDto.CountryId)
        {
            return BadRequest("Invalid Country Id");
        }

        try
        {
            await countriesServices.UpdateCountryAsync(id, updateDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await countriesServices.CountryExistsAsync(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        await countriesServices.UpdateCountryAsync(id, updateDto);

        return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createDto)
    {
        var country = await countriesServices.CreateCountryAsync(createDto);

        return CreatedAtAction("GetCountry", new { id = country.CountryId }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        await countriesServices.DeleteCountryAsync(id);

        return NoContent();
    }
}