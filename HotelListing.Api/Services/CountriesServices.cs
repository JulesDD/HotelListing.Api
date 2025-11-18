using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Country;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace HotelListing.Api.Services;

public class CountriesServices(HotelListingDbContext context, IMapper mapper) : ICountriesServices
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
           .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider)
           .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }

    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await context.Countries
           .Where(c => c.CountryId == id)
           .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();

        return country is null ? Result<GetCountryDto>.NotFound()
            : Result<GetCountryDto>.Success(country);
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {
        try
        {
            if (id != updateDto.CountryId)
            {
                return Result.BadRequest(new ErrorResult("Error", "Invalid Country Id"));
            }

            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new ErrorResult("Error", $"Country '{id}' was not found!"));
            }

            var duplicateCountry = await context.Countries.AnyAsync(c => c.CountryId != id && c.Name == updateDto.Name);
            if (duplicateCountry)
            {
                return Result.Failure(new ErrorResult("Error", $"Country '{updateDto.Name}' already exists in database!"));
            }

            mapper.Map(updateDto, country);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch
        {
            return Result.Failure(new ErrorResult("Error", "An error occurred while updating the country."));
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new ErrorResult("NotFound", $"Country '{id}' was not found!"));
            }
            context.Countries.Remove(country);
            await context.SaveChangesAsync();

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
                return Result<GetCountryDto>.Failure(new ErrorResult("Error", $"'{createDto.Name}' already exists in database!"));
            }

            var country = mapper.Map<Country>(createDto);
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            var dto = new GetCountryDto
            (
               country.CountryId,
               country.Name,
               country.ShortName,
                []
            );

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
        return await context.Countries.AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());
    }
}
