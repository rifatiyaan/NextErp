using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextErp.API;
using NextErp.Application;
using NextErp.Domain.Entities;
using NextErp.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Text;

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
    containerBuilder.RegisterModule(
        new InfrastructureModule(connectionString, migrationAssembly, dbProvider));

    containerBuilder.RegisterModule(new ApplicationModule());
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

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =======================================================
// 🔹 IDENTITY
// =======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// =======================================================
// 🔹 MVC / CONTROLLERS
// =======================================================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

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
// 🔹 CORS
// =======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
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
    app.UseExceptionHandler("/Home/Error");
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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/debug", async context =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.CanConnectAsync();
        await context.Response.WriteAsync("Database connected successfully");
    }
    catch (Exception ex)
    {
        await context.Response.WriteAsync($"Exception: {ex.Message}\n{ex.StackTrace}");
    }
});

app.MapGet("/debug-all", async (HttpContext context) =>
{
    try
    {
        // Example: simulate calling the failing endpoint code
        // Replace this with the actual code that usually causes the 500
        // For example: await someService.DoSomethingAsync();

        await context.Response.WriteAsync("No exception thrown. Everything works!");
    }
    catch (Exception ex)
    {
        // This will show the exception message and stack trace in the browser
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync($"Exception: {ex.GetType().Name}\n");
        await context.Response.WriteAsync($"Message: {ex.Message}\n\n");
        await context.Response.WriteAsync($"Stack Trace:\n{ex.StackTrace}");
    }
});



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
