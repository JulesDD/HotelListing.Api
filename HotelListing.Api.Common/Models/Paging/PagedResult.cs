namespace HotelListing.Api.Common.Models.Paging;

// A generic class to represent paged results, containing the data and pagination metadata
public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMetadata Metadata { get; set; } = new();
}
