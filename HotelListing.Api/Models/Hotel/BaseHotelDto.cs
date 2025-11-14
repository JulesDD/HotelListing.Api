using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.Hotel;

public abstract class BaseHotelDto
{
    public required string Name { get; set; }
    [MaxLength(200)]
    public required string Address { get; set; }
    [Range(1, 10)]
    public int Rating { get; set; }
    [Range(1, int.MaxValue)]
    public required int CountryId { get; set; }
}
