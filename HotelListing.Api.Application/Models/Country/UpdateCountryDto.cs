namespace HotelListing.Api.Application.Models.Country;

public class UpdateCountryDto : BaseCountryDto
{
    public required int CountryId { get; set; }
}