using HotelListing.Api.Configurations;
using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Handlers;
using HotelListing.Api.MappingProfiles;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDBConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICountriesServices, CountriesServices>();     
builder.Services.AddScoped<IHotelsServices, HotelsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<HotelListingDbContext>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(DefaultAuthentication.BasicScheme, _ => {})
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(DefaultAuthentication.ApiKeyScheme, _ => {});
builder.Services.AddAuthorization();

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

//builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(IGenericRepository<>));
//builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
//builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();


var app = builder.Build();

app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

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
