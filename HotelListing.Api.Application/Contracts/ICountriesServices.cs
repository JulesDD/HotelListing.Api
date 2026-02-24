using HotelListing.Api.Common.Result;
using HotelListing.Api.Application.Models.Country;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using Microsoft.AspNetCore.JsonPatch;

namespace HotelListing.Api.Application.Contracts;

public interface ICountriesServices
{
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto);
    Task<Result> DeleteCountryAsync(int id);
    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilteringParameters countryFilteringParameters);
    Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(int id, PaginationParameters paginationParameters, CountryFilteringParameters countryFilteringParameters);
    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDto);
}