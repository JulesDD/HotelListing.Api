using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Application.Models.Country;

public abstract class BaseCountryDto
{
    [MaxLength(25)]
    public required string Name { get; set; }
    [MaxLength(3)]
    public required string ShortName { get; set; }
}
