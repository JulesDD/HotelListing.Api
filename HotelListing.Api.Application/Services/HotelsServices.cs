using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extension;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Result;
using HotelListing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class HotelsService(HotelListingDbContext context, ICountriesServices countriesService, IMapper mapper) : IHotelsServices
{
    public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
    {
        var hotel = await context.Hotels
            .Where(h => h.Id == id)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (hotel is null)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{id}' was not found."));
        }

        return Result<GetHotelDto>.Success(hotel);
    }

    public async Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(PaginationParameters paginationParameters, HotelFilteringParameters hotelFilteringParameters)
    {
        // Apply filters before pagination to ensure accurate metadata and reduce the dataset for pagination, improving performance.
        var query = context.Hotels.AsQueryable();
        // Apply filters conditionally based on the presence of filtering parameters, allowing for flexible querying without enforcing unnecessary conditions.
        if (hotelFilteringParameters.CountryId.HasValue)
        {
            query = query.Where(h => h.CountryId == hotelFilteringParameters.CountryId);
        }
        if (hotelFilteringParameters.MinRating.HasValue)
        {
            query = query.Where(h => h.Rating >= hotelFilteringParameters.MinRating.Value);
        }
        if (hotelFilteringParameters.MaxRating.HasValue)
        {
            query = query.Where(h => h.Rating <= hotelFilteringParameters.MaxRating.Value);
        }
        if (hotelFilteringParameters.MinPrice.HasValue)
        {
            query = query.Where(h => h.PerNightRate >= hotelFilteringParameters.MinPrice.Value);
        }
        if (hotelFilteringParameters.MaxPrice.HasValue)
        {
            query = query.Where(h => h.PerNightRate <= hotelFilteringParameters.MaxPrice.Value);
        }
        if (!string.IsNullOrWhiteSpace(hotelFilteringParameters.Location))
        {
            query = query.Where(h => h.Address.Contains(hotelFilteringParameters.Location));
        }

        // Perform search after applying other filters to reduce the dataset for the search operation, improving performance.
        if (!string.IsNullOrWhiteSpace(hotelFilteringParameters.Search))
        {
            query = query.Where(h => h.Name.ToLower().Contains(hotelFilteringParameters.Search) || h.Address.ToLower().Contains(hotelFilteringParameters.Search));
        }

        // Apply sorting based on the specified field and direction, allowing for dynamic ordering of results while ensuring that sorting is performed after filtering to maintain relevance.
        query = hotelFilteringParameters.SortBy?.ToLower() switch
        {
            "name" => hotelFilteringParameters.SortDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "rating" => hotelFilteringParameters.SortDescending ? query.OrderByDescending(h => h.Rating) : query.OrderBy(h => h.Rating),
            "price" => hotelFilteringParameters.SortDescending ? query.OrderByDescending(h => h.PerNightRate) : query.OrderBy(h => h.PerNightRate),
            _ => query.OrderBy(h=> h.Name)
        };
        var hotel = await query
            .Include(h => h.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);

        return Result<PagedResult<GetHotelDto>>.Success(hotel);
    }

    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var countryExists = await countriesService.CountryExistsAsync(hotelDto.CountryId);
        if (!countryExists)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.NotFound, $"Country '{hotelDto.CountryId}' was not found."));
        }

        var duplicate = await HotelExistsAsync(hotelDto.Name, hotelDto.CountryId);
        if (duplicate)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.Conflict, $"Hotel '{hotelDto.Name}' already exists in the selected country."));
        }

        var hotel = mapper.Map<Hotel>(hotelDto);
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();

        var dto = await context.Hotels
            .Where(h => h.Id == hotel.Id)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .FirstAsync();

        return Result<GetHotelDto>.Success(dto);
    }

    public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Id route value does not match payload Id."));
        }

        var hotel = await context.Hotels.FindAsync(id);
        if (hotel is null)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel '{id}' was not found."));
        }

        var countryExists = await countriesService.CountryExistsAsync(updateDto.CountryId);
        if (!countryExists)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{updateDto.CountryId}' was not found."));
        }

        mapper.Map(updateDto, hotel);

        context.Hotels.Update(hotel);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteHotelAsync(int id)
    {
        var affected = await context.Hotels
            .Where(q => q.Id == id)
            .ExecuteDeleteAsync();

        if (affected == 0)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel '{id}' was not found."));
        }

        return Result.Success();
    }

    public async Task<bool> HotelExistsAsync(int id)
    {
        return await context.Hotels.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> HotelExistsAsync(string name, int countryId)
    {
        return await context.Hotels
            .AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim() && e.CountryId == countryId);
    }
}