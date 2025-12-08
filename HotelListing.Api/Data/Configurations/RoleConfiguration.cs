using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                // Predefined unique identifier for the Administrator role. Generated using a GUID generator.
                Id = "54e5cc4c-074d-46b5-83e4-6753499cba23",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole
            {
                // Predefined unique identifier for the User role. Generated using a GUID generator.
                Id = "0e1cb0b9-c1cf-48c5-b9f5-27d0183c0da4",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                // Predefined unique identifier for the HotelManager role. Generated using a GUID generator.
                Id = "a3f5d6e7-8b9c-4d2e-9f0a-1b2c3d4e5f60",
                Name = "Hotel Administrator",
                NormalizedName = "HOTEL ADMINISTRATOR"
            }
        );
    }
}
