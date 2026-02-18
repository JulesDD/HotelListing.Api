namespace HotelListing.Api.Application.Contracts;

public interface IApiKeyValidatorService
{
    Task<bool> IsValidApiKeyAsync(string apiKey, CancellationToken ct = default);
}