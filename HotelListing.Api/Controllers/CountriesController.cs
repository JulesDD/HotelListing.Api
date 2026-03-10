using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Country;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Filtering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

/// <summary>
/// Controller for managing countries in the Hotel Listing API. 
/// Provides endpoints for CRUD operations on country data, including filtering capabilities for retrieving lists of countries. 
/// Access to certain endpoints is restricted to users with the Administrator role.
/// </summary>
/// <param name="countriesService"></param>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class CountriesController(ICountriesServices countriesService) : BaseApiController
{
    /// <summary>
    /// Retrieves a list of countries based on the provided filtering parameters.
    /// </summary>
    /// <param name="countryFilteringParameters"></param>
    /// <returns></returns>
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries(CountryFilteringParameters countryFilteringParameters)
    {
        var result = await countriesService.GetCountriesAsync(countryFilteringParameters);
        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves a country by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the country to retrieve.</param>
    /// <returns>An ActionResult containing the country data if found; otherwise, a not found result.</returns>
    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await countriesService.GetCountryAsync(id);
        return ToActionResult(result);
    }

    /// <summary>
    /// Updates the details of an existing country.
    /// </summary>
    /// <param name="id">The identifier of the country to update.</param>
    /// <param name="updateDto">The updated country data.</param>
    /// <returns>An IActionResult indicating the outcome of the update operation.</returns>
    // PUT: api/Countries/5
    [HttpPut("{id}")]
    [Authorize(Roles = DefaultRoles.Administrator)]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateDto)
    {
        var result = await countriesService.UpdateCountryAsync(id, updateDto);
        return ToActionResult(result);
    }

    /// <summary>
    /// Applies a JSON Patch to update an existing country.
    /// </summary>
    /// <param name="id">The ID of the country to update.</param>
    /// <param name="patchDto">The JSON Patch document containing the changes to apply.</param>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    // PATCH: api/Countries/5
    [HttpPatch("{id}")]
    [Authorize(Roles = DefaultRoles.Administrator)]
    public async Task<IActionResult> PatchCountry(int id, [FromBody] JsonPatchDocument<UpdateCountryDto> patchDto)
    {
        if(patchDto is null) return BadRequest("Patch document cannot be null.");

        var result = await countriesService.PatchCountryAsync(id, patchDto);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new country and returns the created country data.
    /// </summary>
    /// <remarks>Only users with the Administrator role are authorized to access this endpoint.</remarks>
    /// <param name="createDto">The data for the country to create.</param>
    /// <returns>An ActionResult containing the created country data.</returns>
    // POST: api/Countries
    //Allow only administrators to create new countries. This is enforced using the [Authorize] attribute with the Roles parameter set to "Administrator".
    //Also testing the creation of a new country should be done with an authenticated user who has the Administrator role.
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto createDto)
    {
        var result = await countriesService.CreateCountryAsync(createDto);
        if (!result.IsSuccess) return MapErrorsToResponse(result.Errors);

        return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.CountryId }, result.Value);
    }

    /// <summary>
    /// Deletes a country with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the country to delete.</param>
    /// <returns>An IActionResult indicating the result of the delete operation.</returns>
    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await countriesService.DeleteCountryAsync(id);
        return ToActionResult(result);
    }
}