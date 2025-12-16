namespace HotelListing.Api.Models.Booking;

public record CreateBookingDto
(
    int HotelId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests
);
