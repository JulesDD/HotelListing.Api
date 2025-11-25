using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.Auth;

public class LoginUserDto
{
    [Required, EmailAddress]
    public required string Email { get; set; } = string.Empty;
    [Required]
    public required string Password { get; set; } = string.Empty;
}
