using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextErp.API;
using NextErp.Application;
using NextErp.Application.Common.Behaviors;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Infrastructure;
using NextErp.Infrastructure.Services;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// 🔹 RENDER PORT BINDING (CRITICAL FIX)
// =======================================================
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

// =======================================================
// 🔹 SERILOG
// =======================================================
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration));

// =======================================================
// 🔹 DATABASE CONFIG
// =======================================================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var migrationAssembly =
    Environment.GetEnvironmentVariable("MigrationAssembly")
    ?? "NextErp.Infrastructure"; // Migrations are stored in NextErp.Infrastructure project

var dbProvider =
    builder.Configuration["DatabaseProvider"] ?? "SqlServer";

// =======================================================
// 🔹 AUTOFAC
// =======================================================
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new InfrastructureModule());
    containerBuilder.RegisterModule(new WebModule());
});

// =======================================================
// 🔹 DB CONTEXT
// =======================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString,
            m => m.MigrationsAssembly("NextErp.Infrastructure"));
    }
    else
    {
        options.UseSqlServer(connectionString,
            m => m.MigrationsAssembly(migrationAssembly));
    }
});

// Same scoped instance as ApplicationDbContext (repositories and handlers use IApplicationDbContext).
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IUserContext, HttpContextUserContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =======================================================
// 🔹 IDENTITY
// =======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =======================================================
// 🔹 JWT AUTH
// =======================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// =======================================================
// 🔹 MVC / CONTROLLERS
// =======================================================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opt.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =======================================================
// 🔹 AUTOMAPPER
// =======================================================
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AllowNullDestinationValues = true;
}, typeof(ApplicationAssemblyMarker).Assembly);

// =======================================================
// 🔹 MEDIATR
// =======================================================
builder.Services.AddMediatR(cfg =>
{
    cfg.AddOpenBehavior(typeof(PermissionBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

// =======================================================
// 🔹 SWAGGER
// =======================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "NextERP API",
            Version = "v1"
        });

    c.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

    // Handle nested DTO classes (e.g., Category+Request+Create+Single) to avoid schema ID collisions
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

// =======================================================
// 🔹 CORS (configure Cors:AllowedOrigins in appsettings / env; Development adds localhost in appsettings.Development.json)
// =======================================================
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "CORS is not configured: add at least one origin under Cors:AllowedOrigins (see appsettings.Development.json for local dev).");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =======================================================
// 🔹 BUILD APP
// =======================================================
var app = builder.Build();

// Logs HTTP request/response timings + status codes (very helpful for "loading forever" issues)
app.UseSerilogRequestLogging();

// =======================================================
// 🔹 AUTOMAPPER VALIDATION (DEBUG ONLY)
// =======================================================
#if DEBUG
try
{
    using (var scope = app.Services.CreateScope())
    {
        var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
catch (AutoMapper.AutoMapperConfigurationException ex)
{
    // Log the error but don't crash the app
    var logger = app.Services.GetRequiredService<Serilog.ILogger>();
    logger.Warning(ex, "AutoMapper configuration validation failed. This may be due to unmapped properties.");
    // Uncomment the line below to see the full error details
    // throw;
}
#endif

// =======================================================
// 🔹 MIDDLEWARE PIPELINE
// =======================================================
// app.UseCors("AllowFrontend"); // Moved downstream

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextERP API V1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHsts();
}

// 🔹 IMPORTANT: Disable HTTPS redirect in production (Render)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// =======================================================
// 🔹 ROUTES
// =======================================================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// =======================================================
// 🔹 RUN
// =======================================================
try
{
    Log.Information("Application starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
