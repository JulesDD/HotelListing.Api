using HotelListing.Api.Common.Models.Paging;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Common.Models.Extension;

// Extension methods for IQueryable to support pagination
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, PaginationParameters paginationParameters)
    {
        // Calculate the total number of items in the query
        var totalCount = await source.CountAsync();
        
        // Calculate the total number of pages based on the total items and page size
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);
        
        // Retrieve the data for the specified page using Skip and Take
        var items = await source
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();
        
        // Create pagination metadata to include in the response
        var metadata = new PaginationMetadata
        {
            TotalCount = totalCount,
            PageSize = paginationParameters.PageSize,
            CurrentPage = paginationParameters.PageNumber,
            TotalPages = totalPages,
            HasNext = paginationParameters.PageNumber < totalPages,
            HasPrevious = paginationParameters.PageNumber > 1
        };
        
        // Return a PagedResult containing the data and metadata
        return new PagedResult<T>
        {
            Data = items,
            Metadata = metadata
        };
    }

}
