using Asp.Versioning;
using HotelListing.Api.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.Api.Controllers;

/// <summary>
/// Controller providing endpoints secured with Basic authentication for managing string values.
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
[Authorize(AuthenticationSchemes = DefaultAuthentication.BasicScheme)]
public class BasicAuthController : ControllerBase
{
    /// <summary>
    /// Retrieves a collection of string values.
    /// </summary>
    /// <returns>An enumerable collection containing string values.</returns>
    // GET: api/<ApiKeyController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    /// <summary>
    /// Retrieves a string value for the specified ID.
    /// </summary>
    /// <param name="id">The identifier for which to retrieve the value.</param>
    /// <returns>A string value associated with the given ID.</returns>
    // GET api/<ApiKeyController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    /// <summary>
    /// Handles HTTP POST requests with a string value in the request body.
    /// </summary>
    /// <param name="value">The string value provided in the request body.</param>
    // POST api/<ApiKeyController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    /// <summary>
    /// Updates the resource identified by the specified ID with the provided value.
    /// </summary>
    /// <param name="id">The identifier of the resource to update.</param>
    /// <param name="value">The new value for the resource.</param>
    // PUT api/<ApiKeyController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    /// <summary>
    /// Deletes the API key with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the API key to delete.</param>
    // DELETE api/<ApiKeyController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
