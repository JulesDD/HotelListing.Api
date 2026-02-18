namespace HotelListing.Api.Application.Models.Hotel;

public record GetHotelDto(
    int Id,
    string CountryName,
    string Address,
    double? Rating,
    int CountryId,
    string Country
);

public record GetHotelSlimDto(
    int Id,
    string CountryName,
    string Address,
    double? Rating
    );