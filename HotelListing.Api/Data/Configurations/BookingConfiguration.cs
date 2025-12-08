using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Configure indexes for foreign keys
        builder.HasIndex(b => b.HotelId);
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => new { b.CheckInDate, b.CheckOutDate });

        // Configure enum to be stored as string
        builder.Property(b => b.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
    }
}
