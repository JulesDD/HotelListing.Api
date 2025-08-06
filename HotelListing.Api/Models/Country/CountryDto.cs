namespace HotelListing.Api.Models.Hotel;

public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public List <HotelDto> Hotels { get; set; }
}

