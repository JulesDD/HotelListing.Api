namespace HotelListing.Api.Models.Hotel;

public record GetHotelDto(
    int CountryId,
    string Name,
    string Address,
    int? Rating,
    string Country
);