using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Repositories;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BordspelApp.Api.GraphQL;
using HotChocolate;
using Microsoft.OpenApi.Models;
using BordspelApp.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var domainConnection = builder.Configuration.GetConnectionString("DomainConnection") ?? string.Empty;
var identityConnection = builder.Configuration.GetConnectionString("IdentityConnection") ?? string.Empty;
var containsLocalDb = domainConnection.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
    || domainConnection.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase)
    || identityConnection.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
    || identityConnection.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase);

// Guard: fail fast in Production when still configured to LocalDB
if (builder.Environment.IsProduction() && containsLocalDb)
{
    throw new PlatformNotSupportedException("Production requires separate Azure SQL connections for DomainConnection and IdentityConnection.");
}

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("IdentityCookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Identity.Application",
        Description = "Authenticate with POST /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "IdentityCookie" }
        }] = Array.Empty<string>()
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserAccessor>();
builder.Services.AddAuthorization();

// Configure JSON options for minimal API responses as well
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add GraphQL (HotChocolate v13)
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    // Register DTO types explicitly so HotChocolate uses these runtime types
    // instead of traversing EF entities (prevents EF entities like Person
    // from being treated as input types).
    .AddType<BordspelApp.Api.GraphQL.Models.GameNightDto>()
    .AddType<BordspelApp.Api.GraphQL.Models.BoardGameDto>()
    .AddType<BordspelApp.Api.GraphQL.Models.RecommendationDto>();

// Add DbContext
builder.Services.AddDbContextFactory<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DomainConnection")));

builder.Services.AddDbContext<BordspelAppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Identity - register UserManager/RoleManager so the async seeder can run from the API if desired
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<BordspelAppIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Add Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseForwardedHeaders();
app.UseHttpsRedirection();

// Enable authentication so Identity services are active during requests/startup
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Map GraphQL endpoint for Banana Cake Pop and GraphiQL
app.MapGraphQL("/graphql");

// Conditional development / opt-in seeding
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("SeedDatabase"))
{
    Console.WriteLine("[Seed] Starting database seeding (Development or SeedDatabase=true)...");
    try
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Only run the seeder when Identity services (UserManager<ApplicationUser>) are registered.
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        if (userManager == null)
        {
            Console.WriteLine("[Seed] Skipped: Identity services not registered in API (UserManager<ApplicationUser> missing). Run seeder from WebApp or register Identity in the API.");
        }
        else
        {
            await BordspelApp.Infrastructure.Data.DatabaseSeeder.SeedAsync(serviceProvider);
            Console.WriteLine("[Seed] Database seeding completed successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed] Database seeding failed: {ex}");
        // Don't rethrow - allow the app to start even if seeding fails in the API.
    }
}

app.Run();

namespace BordspelApp.Api
{
    // Public entry point for WebApplicationFactory-based integration tests.
    public partial class Program
    {
    }
}
