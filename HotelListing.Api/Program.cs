using Asp.Versioning;
using HealthChecks.UI.Client;
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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
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

    builder.Services.AddHealthChecks()
        .AddCheck("Self", () => HealthCheckResult.Healthy("The API is healthy!"), tags: ["api"])
        .AddDbContextCheck<HotelListingDbContext>(
        name: "Database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql"]);
    
    builder.Services.AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(10);
        setup.MaximumHistoryEntriesPerEndpoint(50);
        setup.AddHealthCheckEndpoint("HotelListing API", "/healthz");
    })
    .AddInMemoryStorage();

    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(opt =>
    {
        // API Information
        opt.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Hotel Listing API",
            Description = "An ASP.NET Core Web API for managing hotels and countries.",
            Contact = new OpenApiContact
            {
                Name = "Jules Douglas",
                Email = "jules.douglas@hotmail.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Include XML comments if available
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            opt.IncludeXmlComments(xmlPath);
        }

        // Enable annotions for API versioning
        opt.EnableAnnotations();


        // Add JWT Authentication to Swagger
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: ",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        // Add API Key Authentication to Swagger
        opt.AddSecurityDefinition(DefaultAuthentication.ApiKeyScheme, new OpenApiSecurityScheme
        {
            Description = "API Key Authentication.",
            Name = "X-API-KEY",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = DefaultAuthentication.ApiKeyScheme
        });

        // Add Basic Authentication to Swagger
        opt.AddSecurityDefinition(DefaultAuthentication.BasicScheme, new OpenApiSecurityScheme
        {
            Description = "Basic Authentication.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = DefaultAuthentication.BasicScheme,
            BearerFormat = "JWT"
        });

        // Add security requirements for all three authentication schemes
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{ }
            },
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = DefaultAuthentication.ApiKeyScheme
                    }
                },
                new string[]{ }
            },
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = DefaultAuthentication.BasicScheme
                    }
                },
                new string[]{ }
            }
        });

        // Add filters to include examples and apply security requirements
        opt.ExampleFilters();

        // Apply the security requirements to all operations
        opt.OperationFilter<HotelListing.Api.Filters.SecurityRequirementsOperationFilters>();

        // Order actions by method and then by path
        opt.OrderActionsBy((apiDesc) => $"{apiDesc.HttpMethod}_{apiDesc.RelativePath}");

    });

    builder.Services.AddMemoryCache();

    builder.Services.AddOutputCache();

    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    var app = builder.Build();

    app.UseExceptionHandler("/error");

    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.MapOpenApi();
        app.UseSwaggerUI(opt =>
        {
            opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Listing API v1");
            opt.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
            opt.DocumentTitle = "Hotel Listing API Documentation";
            opt.DisplayRequestDuration();
            opt.EnableDeepLinking();
            opt.EnableFilter();
            opt.ShowExtensions();
            opt.EnableValidator();
        });
    }

   

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

    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    exception = e.Value.Exception?.Message,
                    duration = e.Value.Duration.ToString()
                })
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    });

    app.MapHealthChecks("/healthz/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
    {
        Predicate = report => report.Tags.Contains("db")
    });

    app.MapHealthChecks("/healthz_ui", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/healthchecks-ui";
        options.ApiPath = "/healthchecks-api";
    });

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


