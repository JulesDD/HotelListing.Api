using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.MappingProfiles;
using HotelListing.Api.Application.Services;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Config;
using HotelListing.Api.Configurations;
using HotelListing.Api.Domain;
using HotelListing.Api.Handlers;
using HotelListing.Api.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();
try 
{
    Log.Information("Starting HotelListing API...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, service, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(service)
        );

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("HotelListingDBConnectionString");


    builder.Services.AddDbContext<HotelListingDbContext>(options => {
        options.UseSqlServer(connectionString, sqlOptions => {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        });
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    builder.Services.AddScoped<ICountriesServices, CountriesServices>();
    builder.Services.AddScoped<IHotelsServices, HotelsService>();
    builder.Services.AddScoped<IUsersService, UsersService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter(RateLimitingConstants.FixedPolicy, opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.AddPolicy(RateLimitingConstants.PerUserPolicy, context =>
        {
            var userName = context.User?.Identity?.Name ?? "anonymous";
            return RateLimitPartition.GetFixedWindowLimiter(userName, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 3
            });
        });

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests. Please try again later.",
                message = "Rate limit exceeded. Please wait before making more requests.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken: cancellationToken);
        };
    });

    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddEntityFrameworkStores<HotelListingDbContext>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        Log.Fatal("JWT settings are not properly configured. Please check the configuration.");
        throw new InvalidOperationException("JWT settings are not properly configured.");
    }
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
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        })
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(DefaultAuthentication.BasicScheme, _ => { })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(DefaultAuthentication.ApiKeyScheme, _ => { });
    builder.Services.AddAuthorization();

    builder.Services.AddAutoMapper(typeof(MappingProfileHotel).Assembly);
    builder.Services.AddAutoMapper(typeof(MappingProfileCountry).Assembly);
    builder.Services.AddAutoMapper(typeof(BookingMappingProfile).Assembly);

    builder.Services.AddControllers()
        .AddNewtonsoftJson()
        .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
    });

    builder.Services.AddAutoMapper(typeof(MapperConfig));

    var app = builder.Build();

    app.UseExceptionHandler("/error");

    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    builder.Services.AddMemoryCache();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null || httpContext.Response.StatusCode >= 500)
                return Serilog.Events.LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 400)
                return Serilog.Events.LogEventLevel.Warning;
            return Serilog.Events.LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "anonymous");
            if(httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                diagnosticContext.Set("UserId", userId);
            }
        };
    });


    app.UseHttpsRedirection();

    app.UseRateLimiter();
    
    app.UseAuthorization();
    
    app.UseOutputCache();

    app.MapControllers();

    app.UseCors("AllowAll");
   
    Log.Information("HotelListing API is running...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


