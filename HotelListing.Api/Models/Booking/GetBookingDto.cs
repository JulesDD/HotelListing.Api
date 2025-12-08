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

public record CreateBookingDto
(
    int HotelId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests
);

public record UpdateBookingDto
(
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    string Status
);
