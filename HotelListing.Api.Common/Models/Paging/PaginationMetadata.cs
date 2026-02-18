namespace HotelListing.Api.Common.Models.Paging;

// A class to hold metadata about the pagination, such as total count, page size, current page, total pages, and navigation flags
public class PaginationMetadata
{
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }

}