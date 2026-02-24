using HotelListing.Api.Common.Enums;

namespace HotelListing.Api.Common.Models.Filtering;

public class BookingFilteringParameters : BaseFilteringParameters
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinGuest { get; set; }
    public int? MaxGuest { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? CheckInTo { get; set; }
    public DateTime? CheckInFrom { get; set; }
    public DateTime? CheckOutTo { get; set; }
    public DateTime? CheckOutFrom { get; set; }
    public BookingStatus? Status { get; set; }
}
