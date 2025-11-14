using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Api.Data;

public class Hotel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    [Range(1, 10)]
    public int Rating { get; set; }
    [ForeignKey(nameof(CountryId))]
    public required int CountryId { get; set; }
    public Country? Country { get; set; }

}
