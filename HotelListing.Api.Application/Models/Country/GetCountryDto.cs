using HotelListing.Api.Application.Models.Hotel;

namespace HotelListing.Api.Application.Models.Country;

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