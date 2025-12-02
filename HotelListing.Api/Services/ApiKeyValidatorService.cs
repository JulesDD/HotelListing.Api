using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

//Inject the DbContext if you want to validate against a database
public class ApiKeyValidatorService(HotelListingDbContext db) : IApiKeyValidatorService
{
    public async Task<bool> IsValidApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        // Implement your API key validation logic here
        // For example, check against a database or a predefined list of valid keys
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }
        var apiKeyEntity = await db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == apiKey, ct);

        if (apiKeyEntity == null)
        {
            return false;

        }
        return apiKeyEntity.IsActive;
    }
}
