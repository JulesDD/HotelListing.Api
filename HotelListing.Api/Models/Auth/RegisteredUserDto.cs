namespace HotelListing.Api.Models.Auth;

public class RegisteredUserDto
{
    public required string Id { get; set; } = string.Empty;
    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public string? Role { get; set; } = "User";
}