namespace HotelListing.Api.Models.Booking;

public record GetBookingDto
(
    int Id,
    int HotelId,
    string HotelName,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
