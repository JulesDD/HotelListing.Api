using HotelListing.Api.Contracts;

namespace HotelListing.Api.Services;

//Inject the DbContext if you want to validate against a database
public class ApiKeyValidatorService(IConfiguration configuration) : IApiKeyValidatorService
{
    public Task<bool> IsValidApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        // Implement your API key validation logic here
        // For example, check against a database or a predefined list of valid keys
        return Task.FromResult(apiKey.Equals(configuration["ApiKey"], StringComparison.Ordinal));
    }
}
