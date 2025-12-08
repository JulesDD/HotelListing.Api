using HotelListing.Api.Data.Enums;

namespace HotelListing.Api.Data;

public class Booking 
{
    public int Id { get; set; }
    public required int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAtUtc { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
