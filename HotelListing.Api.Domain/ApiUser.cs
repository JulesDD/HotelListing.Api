using Microsoft.AspNetCore.Identity;

namespace HotelListing.Api.Domain;

public class ApiUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}
