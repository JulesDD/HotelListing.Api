namespace HotelListing.Api.Common.Models.Filtering;

public class HotelFilteringParameters : BaseFilteringParameters
{
    public int? CountryId { get; set; }
    public double? MinRating { get; set; }
    public double? MaxRating { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Location { get; set; }
}
