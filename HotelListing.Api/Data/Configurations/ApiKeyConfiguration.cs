using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(a => a.Key).IsUnique();
        builder.HasData(
            new ApiKey
            {
                Id = 1,
                AppName = "Service A",
                CreatedAtUtc = new DateTime(2025,12,01),
                Key = "QWRtaW5AbG9jYWxob3N0LmNvbTpQYXNzd29yZDE="

            }
        );
    }
}
