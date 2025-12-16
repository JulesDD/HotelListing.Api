namespace HotelListing.Api.Models.Booking;

public record UpdateBookingDto
(
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    string Status
);
