namespace HotelListing.Api.Models.Hotel;

public class CountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public List <HotelDto> Hotels { get; set; }
}

