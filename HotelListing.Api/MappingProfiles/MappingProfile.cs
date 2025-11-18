using AutoMapper;
using HotelListing.Api.Models.Hotel;
using HotelListing.Api.Models.Country;
using HotelListing.Api.Data;

namespace HotelListing.Api.MappingProfiles;

public class MappingProfileHotel : Profile
{
    public MappingProfileHotel()
    {
        CreateMap<Hotel, GetHotelDto>()
            .ForMember(d => d.Country, cfg => cfg.MapFrom<ResolveCountryName>());

        CreateMap<CreateHotelDto, Hotel>();
    }
}

public class MappingProfileCountry : Profile
{
    public MappingProfileCountry()
    {
        CreateMap<Country, GetCountryDto>();
        CreateMap<Country, GetCountriesDto>();
        CreateMap<CreateCountryDto, Country>();
    }
}

//Resolving an issue where Country name was not being mapped in Hotel to GetHotelDto
public class ResolveCountryName : IValueResolver<Hotel, GetHotelDto, string>
{
    public string Resolve(Hotel source, GetHotelDto destination, string destMember, ResolutionContext context)
    {
        return source.Country?.Name ?? string.Empty;
    }
}
