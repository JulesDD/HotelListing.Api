using HotelListing.Api.Application.Models.Hotel;
using HotelListing.Api.Common.Models.Paging;

namespace HotelListing.Api.Application.Models.Country;

public class GetCountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public List<GetHotelSlimDto> Hotels { get; set; } = new();
};

public class GetCountryHotelsDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PagedResult<GetHotelSlimDto> Hotels { get; set; } = new();
};

public class GetCountriesDto
{
    public int CountryId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
};