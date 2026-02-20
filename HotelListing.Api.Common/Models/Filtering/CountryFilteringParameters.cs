namespace HotelListing.Api.Common.Models.Filtering;

public class CountryFilteringParameters : BaseFilteringParameters
{
    public string? CountryName { get; set; }
    public bool? HasHotels { get; set; }
}
