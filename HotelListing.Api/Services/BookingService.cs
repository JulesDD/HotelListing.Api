using HotelListing.Api.Data;
using HotelListing.Api.Results;
using HotelListing.Api.Models.Booking;
using HotelListing.Api.Data.Enums;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Contracts;

namespace HotelListing.Api.Services;

public class BookingService(HotelListingDbContext context, IHttpContextAccessor httpContextAccessor) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetHotelBookingsAsync(int hotelId)
    {
        // Check if the hotel exists
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        // Retrieve bookings for the specified hotel ordered by check-in date
        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckInDate)
            .Select(b => new GetBookingDto
            (
                b.Id,
                b.HotelId,
                b.Hotel!.Name,
                b.CheckInDate,
                b.CheckOutDate,
                b.NumberOfGuests,
                b.TotalPrice,
                b.Status.ToString(),
                b.CreatedAtUtc,
                b.UpdatedAtUtc
            ))
            .ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateBookingsAsync(CreateBookingDto createBookingDto)
    {
        // Get user ID from JWT claims
        var userId = httpContextAccessor?.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "User is not authorized."));
        }

        // Validate booking dates by calculating the number of nights
        var nights = (createBookingDto.CheckOutDate - createBookingDto.CheckInDate).Days;
        if (nights <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "Check-out date must be after check-in date."));
        }

        // Check the amount of guests
        if (createBookingDto.NumberOfGuests <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "Number of guests must be at least 1."));
        }

        // Check if the hotel exists
        var hotel = await context.Hotels
            .Where(h => h.Id == createBookingDto.HotelId)
            .FirstOrDefaultAsync();
        if (hotel == null)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{createBookingDto.HotelId}' was not found."));
        }

        // Check for overlapping bookings
        var overlappingBookingExists = await context.Bookings
            .AnyAsync(b => b.HotelId == createBookingDto.HotelId &&
                           b.Status == BookingStatus.Confirmed &&
                           b.CheckInDate < createBookingDto.CheckOutDate &&
                           b.CheckOutDate > createBookingDto.CheckInDate &&
                           b.UserId == userId);
        if (overlappingBookingExists)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with an existing booking."));
        }

        // Calculate total price and create the booking
        var totalPrice = nights * hotel.PerNightRate;
        var booking = new Booking
        {
            HotelId = createBookingDto.HotelId,
            UserId = userId,
            CheckInDate = createBookingDto.CheckInDate,
            CheckOutDate = createBookingDto.CheckOutDate,
            NumberOfGuests = createBookingDto.NumberOfGuests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Prepare the result DTO
        var createdBookings = new GetBookingDto
        (
            booking.Id,
            hotel.Id,
            hotel.Name,
            createBookingDto.CheckInDate,
            createBookingDto.CheckOutDate,
            createBookingDto.NumberOfGuests,
            totalPrice,
            BookingStatus.Pending.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
        );

        return Result<GetBookingDto>.Success(createdBookings);
    }
}
