namespace HotelListing.Api.Application.Models.Hotel;

public class UpdateHotelDto : BaseHotelDto
{
    public required int Id { get; set; }
}