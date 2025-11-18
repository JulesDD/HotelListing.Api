using HotelListing.Api.Models.Hotel;
using HotelListing.Api.Data;
using HotelListing.Api.Contracts;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace HotelListing.Api.Services;

public class HotelsServices(HotelListingDbContext context, IMapper mapper) : IHotelsServices
{
    public async Task<IEnumerable<GetHotelDto>> GetHotelsAsync()
    {
        var hotels = await context.Hotels
            .Include(q => q.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return hotels;
    }

    public  async Task<GetHotelDto?> GetHotelAsync(int id)
    {
        var hotel = await context.Hotels
            .Where(h => h.Id == id)
            .Include(q => q.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(h => h.CountryId == id);

        return hotel ?? null;
    }

    public async Task UpdateHotelAsync(int id, UpdateHotelDto updateDto)
    {
        var hotel = await context.Hotels.FindAsync(id) ?? throw new KeyNotFoundException("Hotel not found");
        var returnHotelObject = mapper.Map<UpdateHotelDto, Hotel>(updateDto, hotel);

        context.Entry(hotel).State = EntityState.Modified;

        context.Hotels.Update(hotel);

        await context.SaveChangesAsync();
    }

    public async Task DeleteHotelAsync(int id)
    {
        var hotel = await context.Hotels.Where(h => h.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<GetHotelDto> CreateHotelAsync(CreateHotelDto createDto)
    {
        var hotel = mapper.Map<Hotel>(createDto);
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        var returnHotelObject = mapper.Map<GetHotelDto>(hotel);
        
        return returnHotelObject;
    }

    public async Task<bool> HotelExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }

    public async Task<bool> HotelExistsAsync(string name)
    {
        return await context.Countries.AnyAsync(e => e.Name == name);
    }
}
