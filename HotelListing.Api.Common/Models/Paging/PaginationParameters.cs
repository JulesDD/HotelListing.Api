using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelListing.Api.Common.Models.Paging;

public class PaginationParameters
{
    // Set a maximum page size to prevent excessive data retrieval
    private const int MaxPageSize = 50;
    
    // Default page size if not specified by the client
    private int _pageSize = 10;

    // The page number to retrieve, defaulting to 1
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
    public int PageNumber { get; init; } = 1;
    
    // The number of items per page, with validation to ensure it does not exceed the maximum
    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 50")]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}
