using HotelListing.Api.Models.Hotel;

namespace HotelListing.Api.Models.Country;

public record GetCountryDto(
    int CountryId,
    string Name,
    string ShortName,
    List<GetHotelSlimDto>? Hotels
);

public record GetCountriesDto(
    int CountryId,
    string Name,
    string ShortName
);