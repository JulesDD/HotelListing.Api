using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.Api.AuthorizationFilters;

// Custom attribute to apply the AdminFilter
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AdminAttributes : TypeFilterAttribute
{
    public AdminAttributes() : base(typeof(AdminFilter))
    {
    }
}


public class AdminFilter(HotelListingDbContext hotelListingDbContext) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        //Determine if the user is an admin
        var user = context.HttpContext.User;

        // If the user is not authenticated, return 401 Unauthorized
        if (!user.Identity?.IsAuthenticated ?? false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if the user has the "Administrator" role
        if (!user.IsInRole("Administrator"))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Optionally, you can retrieve the user ID from the claims if needed
        var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Try to get hotelId from route data 
        context.RouteData.Values.TryGetValue("hotelId", out var hotelIdObj);
        int.TryParse(hotelIdObj?.ToString(), out var parsedHotelId);
        if (parsedHotelId == 0)
        {
            context.Result = new BadRequestObjectResult("Invalid hotel ID.");
            return;
        }

        // Check if the user is associated with the hotel as a Hotel Administrator
        var isHotelAdmin = await hotelListingDbContext.HotelAdmins
            .AnyAsync(ha => ha.HotelId == parsedHotelId && ha.UserId == userId);
        if (!isHotelAdmin)
        {             
            context.Result = new ForbidResult();
            return;
        }
    }
}
