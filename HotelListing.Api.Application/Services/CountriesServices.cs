using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Models.Country;
using HotelListing.Api.Application.Models.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extension;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Result;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelListing.Api.Application.Services;

public class CountriesServices(HotelListingDbContext context, IMapper mapper, IMemoryCache cache) : ICountriesServices
{
    private const string CountriesListCacheKey = "countries_list";
    private const string CountriesListCacheName = "Country_";

    private void InvalidateCountriesCache(int id)
    {
        cache.Remove($"{CountriesListCacheName}{id}");
    }
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilteringParameters? countryFilteringParameters)
    {
        // Start with the base query for countries
        var search = countryFilteringParameters?.Search?.Trim().ToLowerInvariant() ?? string.Empty;
        var cacheKey = $"{CountriesListCacheKey}{ search}";

        if(!cache.TryGetValue(cacheKey, out IEnumerable<GetCountriesDto>? cachedCountries))
        {
            var query = context.Countries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(countryFilteringParameters?.Search))
            {
                var term = countryFilteringParameters.Search.Trim().ToLower();
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%") || EF.Functions.Like(c.ShortName, $"%{term}%"));
            }

            cachedCountries = await query
                .AsNoTracking()
                .Where(c => string.IsNullOrWhiteSpace(search) || EF.Functions.Like(c.Name.ToLower(), $"%{search}%") || EF.Functions.Like(c.ShortName.ToLower(), $"%{search}%"))
                .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider)
                .ToListAsync();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) 
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            cache.Set(cacheKey, cachedCountries, cacheEntryOptions);
        }

        cachedCountries ??= [];

        return Result<IEnumerable<GetCountriesDto>>.Success(cachedCountries);
    }

    public async Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(int countryId, PaginationParameters paginationParameters, CountryFilteringParameters filters)
    {
        var exists = await CountryExistsAsync(countryId);
        if (!exists)
        {
            return Result<GetCountryHotelsDto>.Failure(
                new Error(ErrorCodes.NotFound, $"Country '{countryId}' was not found."));
        }

        var countryName = await context.Countries
            .AsNoTracking()
            .Where(q => q.CountryId == countryId)
            .Select(q => q.Name)
            .SingleAsync();

        var hotelsQuery = context.Hotels
            .Where(h => h.CountryId == countryId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            hotelsQuery = hotelsQuery.Where(h => EF.Functions.Like(h.Name, $"%{term}%"));
        }

        hotelsQuery = (filters.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => filters.SortDescending ? hotelsQuery.OrderByDescending(h => h.Name) : hotelsQuery.OrderBy(h => h.Name),
            "rating" => filters.SortDescending ? hotelsQuery.OrderByDescending(h => h.Rating) : hotelsQuery.OrderBy(h => h.Rating),
            _ => hotelsQuery.OrderBy(h => h.Name)
        };

        var pagedHotels = await hotelsQuery
            .ProjectTo<GetHotelSlimDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);

        var result = new GetCountryHotelsDto
        {
            CountryId = countryId,
            Name = countryName,
            Hotels = pagedHotels
        };

        return Result<GetCountryHotelsDto>.Success(result);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        // Try to get the country from cache first
        var cacheKey = $"{CountriesListCacheName}{id}";
        if (!cache.TryGetValue(cacheKey, out GetCountryDto? country))
        {
            // If not found in cache, query the database
            country = await context.Countries
               .AsNoTracking()
               .Where(c => c.CountryId == id)
               .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();

            // If found in database, store it in cache for future requests
            if (country is not null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) 
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                cache.Set(cacheKey, country, cacheEntryOptions); 
            }

        }

        return country is null ? Result<GetCountryDto>.NotFound()
            : Result<GetCountryDto>.Success(country);
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {
        try
        {
            if (id != updateDto.CountryId)
            {
                return Result.BadRequest(new Error(ErrorCodes.Validation, "Id route value does not math payload Id"));
            }

            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found!"));
            }

            var duplicateCountry = await context.Countries.AnyAsync(c => c.CountryId != id && c.Name == updateDto.Name);
            if (duplicateCountry)
            {
                return Result.Failure(new Error(ErrorCodes.Failure, $"Country '{updateDto.Name}' already exists in database!"));
            }

            mapper.Map(updateDto, country);
            await context.SaveChangesAsync();
            InvalidateCountriesCache(id);

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error(ErrorCodes.Failure, "An error occurred while updating the country."));
        }
    }

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDto)
    {
        try
        {
            // Check if the country exists
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found!"));
            }

            // Map the existing country to an UpdateCountryDto to apply the patch
            var countryToPatch = mapper.Map<UpdateCountryDto>(country);
            patchDto.ApplyTo(countryToPatch);

            // Check for duplicate country name after patching
            var normalizedName = countryToPatch.Name?.Trim().ToLower();
            var duplicateCountry = await context.Countries.AnyAsync(c => c.CountryId != id && c.Name.ToLower().Trim() == normalizedName);
            if (duplicateCountry)
                return Result.Failure(new Error(ErrorCodes.Failure, $"Country '{countryToPatch.Name}' already exists in database!"));

            // Map the patched DTO back to the country entity and save changes
            mapper.Map(countryToPatch, country);
            await context.SaveChangesAsync();
            return Result.Success();
        }
        catch
        {
            return Result.Failure(new Error(ErrorCodes.Failure, "An error occurred while patching the country."));
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found!"));
            }
            context.Countries.Remove(country);
            await context.SaveChangesAsync();
            InvalidateCountriesCache(id);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure();
        }
    }

    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto)
    {
        try
        {
            var existingCountry = await CountryExistsAsync(createDto.Name);
            if (existingCountry)
            {
                return Result<GetCountryDto>.Failure(new Error(ErrorCodes.Failure, $"'{createDto.Name}' already exists in database!"));
            }

            // Map the CreateCountryDto to a Country entity, add it to the context, and save changes
            var country = mapper.Map<Country>(createDto);
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            // Map the newly created Country entity back to a GetCountryDto to return in the response
            var dto = mapper.Map<GetCountryDto>(country);

            // Invalidate the countries list cache
            cache.Remove(CountriesListCacheKey); 

            return Result<GetCountryDto>.Success(dto);

        }

        catch (Exception)
        {
            return Result<GetCountryDto>.Failure();
        }
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }

    public async Task<bool> CountryExistsAsync(string name)
    {
        return await context.Countries.AsNoTracking().AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());
    }
}
