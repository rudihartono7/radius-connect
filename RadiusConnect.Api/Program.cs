using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RadiusConnect.Api.Models.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RadiusConnect.Api.Data;
using RadiusConnect.Api.Infrastructure.Authentication;
using RadiusConnect.Api.Infrastructure.Authorization;
using RadiusConnect.Api.Infrastructure.HealthChecks;
using RadiusConnect.Api.Infrastructure.Logging;
using RadiusConnect.Api.Middleware;
using RadiusConnect.Api.Repositories;
using RadiusConnect.Api.Repositories.Interfaces;
using RadiusConnect.Api.Services;
using RadiusConnect.Api.Services.Interfaces;

using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.SpaServices;
using RadiusConnect.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

// RADIUS Database Configuration
builder.Services.AddDbContext<RadiusDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("RadiusConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

// Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

// Add SPA services
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/dist";
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Authentication failed");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.Identity?.Name;
            logger.LogDebug("Token validated for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("UserOrAbove", policy => policy.RequireRole("Admin", "Manager", "User"));
});



// Infrastructure Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRadiusRepository, RadiusRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// Authentication & Authorization Services
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();

// Business Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRadiusService, RadiusService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IJwtService, JwtService>();


// Logging Services
builder.Services.AddStructuredLogging();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                new[] { "http://localhost:3000", "https://localhost:3000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RadiusConnect API",
        Version = "v1",
        Description = "A comprehensive FreeRADIUS management API with modern authentication and monitoring capabilities.",
        Contact = new OpenApiContact
        {
            Name = "RadiusConnect Support",
            Email = "support@radiusconnect.com"
        }
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});


// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<RadiusDatabaseHealthCheck>("radius_database");

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();


app.UseStaticFiles();
app.UseSpaStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RadiusConnect API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("AllowNuxtDev");
}

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    if (app.Environment.IsProduction())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});


app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseRateLimiter();

// Custom JWT Middleware (runs before authentication)
app.UseJwtMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapHealthChecks("/health");

app.UseWhen(
    static context =>  
    !context.Request.Path.StartsWithSegments("/swagger") &&
    !context.Request.Path.StartsWithSegments("/api") &&
    !context.Request.Path.StartsWithSegments("/health") && 
    !context.Request.Path.StartsWithSegments("/uploads"), app =>
{
  app.UseSpaStaticFiles();
  app.UseSpa(spa =>
  {
    spa.Options.SourcePath = "ClientApp";
    
    if (builder.Environment.IsDevelopment())
    {
        spa.UseNuxtDevelopmentServer();
    }
    else
    {
        // Production configuration - serve the built Nuxt app
        spa.Options.DefaultPage = "/index.html";
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // Prevent caching of the main index.html file
                if (ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                    ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                    ctx.Context.Response.Headers.Append("Expires", "0");
                }
            }
        };
    }
  });
});

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();        // Ensure database is created and migrated
        await context.Database.MigrateAsync();

        // Seed initial data
        await SeedDataAsync(context, userManager, roleManager, logger);

        logger.LogInformation("Database migration and seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

app.Run();

// Data Seeding Method
static async Task SeedDataAsync(
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger logger)
{
    // Seed Identity Roles
    var roles = new[] { "Admin", "Manager", "User" };
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole<Guid> { Name = roleName };
            await roleManager.CreateAsync(role);
            logger.LogInformation("Created Identity role: {RoleName}", roleName);
        }
    }

    // Seed RBAC Roles
    var rbacRoles = new[]
    {
        new { Name = "Admin", Description = "Full system access", Permissions = new[] { "users.read", "users.create", "users.update", "users.delete", "groups.read", "groups.create", "groups.update", "groups.delete", "sessions.read", "sessions.manage", "sessions.disconnect", "radius.read", "radius.create", "radius.update", "radius.delete", "dashboard.read", "dashboard.manage", "audit.read", "audit.export", "audit.manage", "system.manage", "system.configure" } },
        new { Name = "Manager", Description = "Management access", Permissions = new[] { "users.read", "users.create", "users.update", "groups.read", "groups.create", "groups.update", "sessions.read", "sessions.manage", "sessions.disconnect", "radius.read", "radius.create", "radius.update", "dashboard.read", "audit.read", "audit.export" } },
        new { Name = "User", Description = "Basic user access", Permissions = new[] { "dashboard.read", "sessions.read" } }
    };

    foreach (var roleData in rbacRoles)
    {
        var existingRole = await context.RbacRoles.FirstOrDefaultAsync(r => r.Name == roleData.Name);
        if (existingRole == null)
        {
            var rbacRole = new RbacRole
            {
                Name = roleData.Name,
                Description = roleData.Description,
                Permissions = roleData.Permissions.ToList()
            };
            context.RbacRoles.Add(rbacRole);
            logger.LogInformation("Created RBAC role: {RoleName}", roleData.Name);
        }
    }

    await context.SaveChangesAsync();

    // Seed Admin User
    var adminEmail = "admin@radiusconnect.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "admin",
            Email = adminEmail,
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Created admin user: {Email}", adminEmail);
        }
        else
        {
            logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Assign admin user to RBAC Admin role
    if (adminUser != null)
    {
        var adminRbacRole = await context.RbacRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRbacRole != null)
        {
            var existingUserRole = await context.RbacUserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRbacRole.Id);
            
            if (existingUserRole == null)
            {
                var userRole = new RbacUserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRbacRole.Id,
                    AssignedAt = DateTime.UtcNow
                };
                context.RbacUserRoles.Add(userRole);
                logger.LogInformation("Assigned admin user to RBAC Admin role");
            }
        }
    }

    await context.SaveChangesAsync();
}
