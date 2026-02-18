using AutoMapper;
using HotelListing.Api.Application.Models.Country;
using HotelListing.Api.Application.Models.Hotel;
using HotelListing.Api.Domain;

namespace HotelListing.Api.Configurations;

public class MapperConfig: Profile
{
	public MapperConfig()
	{
		CreateMap<Country, CreateCountryDto>().ReverseMap();
		CreateMap<Country, GetCountryDto>().ReverseMap();
        CreateMap<Country, CountryDto>().ReverseMap();
		CreateMap<Country, UpdateCountryDto>().ReverseMap();

        CreateMap<Hotel, CreateHotelDto>().ReverseMap();
        CreateMap<Hotel, UpdateHotelDto>().ReverseMap();
        CreateMap<Hotel, BaseHotelDto>().ReverseMap();
        CreateMap<Hotel, HotelDto>().ReverseMap();
    }
}

