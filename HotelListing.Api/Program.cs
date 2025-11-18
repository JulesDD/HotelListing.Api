using HotelListing.Api.Configurations;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Repository;
using HotelListing.Api.Services;
using HotelListing.Api.MappingProfiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDBConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>  options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICountriesServices, CountriesServices>()     builder.Services.AddScoped<IHotelsServices, HotelsServices>();

builder.Services.AddIdentityCore<ApiUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<HotelListingDbContext>();

builder.Services.AddAutoMapper(typeof(MappingProfileHotel).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfileCountry).Assembly);

builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b=> b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddAutoMapper(typeof(MapperConfig));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(IGenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
