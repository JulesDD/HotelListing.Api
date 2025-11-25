using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.Auth;

public class RegisterUserDto
{
    [Required, MaxLength(100)]
    public required string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public required string LastName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public required string Email { get; set; } = string.Empty;
    [Required, MinLength(8)]
    public required string Password { get; set; } = string.Empty;
}
