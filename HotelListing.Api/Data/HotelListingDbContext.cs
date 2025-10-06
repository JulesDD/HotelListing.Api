using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Data;

public class HotelListingDbContext : IdentityDbContext<ApiUser>
{
    public HotelListingDbContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Country> Countries { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Country>().HasData(
            new Country
            {
                Id = 1,
                Name = "Cananda",
                ShortName = "CAN"
            },
            new Country
            {
                Id = 2,
                Name = "England",
                ShortName = "ENG"
            }
            );
        modelBuilder.Entity<Hotel>().HasData(
            new Hotel
            {
                Id = 1,
                Name = "Marriot Hotel",
                Address = "Toronto",
                CountryId = 1,
                Rating = 4.5
            },
            new Hotel
            {
                Id= 2,
                Name = "Fairmont Chateau Whistler",
                Address = "Whistler",
                CountryId = 1,
                Rating = 4.7
            },
            new Hotel
            {
                Id = 3,
                Name = "The Grand Hotel Birminham",
                Address = "Birmingham",
                CountryId = 2,
                Rating = 4.6
            },
            new Hotel
            {
                Id = 4,
                Name = "Hampton by Hilton London Stanstead",
                Address = "London",
                CountryId = 2,
                Rating = 4.3
            }
            );
    }
}
