using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Repositories;
using BordspelApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BordspelApp.Api.GraphQL;
using HotChocolate;

var builder = WebApplication.CreateBuilder(args);

// Startup diagnostics for connection string selection (temporary)
static string MaskConnectionString(string cs)
{
    if (string.IsNullOrWhiteSpace(cs)) return string.Empty;
    var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(p =>
        {
            var idx = p.IndexOf('=');
            if (idx <= 0) return p;
            var key = p.Substring(0, idx).Trim();
            var val = p.Substring(idx + 1);
            var lk = key.ToLowerInvariant();
            if (lk.Contains("password") || lk.Contains("pwd") || lk.Contains("user id") || lk == "user" || lk == "uid")
                return key + "=***";
            return key + "=" + val;
        })
        .ToArray();
    return string.Join(";", parts);
}

var _envName = builder.Environment.EnvironmentName;
// Check common env var sources that Azure may inject
var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
           ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection");
var usedSource = !string.IsNullOrWhiteSpace(envConn) ? "env var (ConnectionStrings__DefaultConnection/SQLAZURECONNSTR_*)" : (string.IsNullOrWhiteSpace(cfgConn) ? "none" : "config (ConnectionStrings:DefaultConnection)");
var rawConn = !string.IsNullOrWhiteSpace(envConn) ? envConn : cfgConn ?? string.Empty;
var masked = MaskConnectionString(rawConn);
var containsLocalDb = rawConn.IndexOf("(localdb)", StringComparison.OrdinalIgnoreCase) >= 0 || rawConn.IndexOf("mssqllocaldb", StringComparison.OrdinalIgnoreCase) >= 0;
Console.WriteLine($"STARTUP: ENV={_envName}; Source={usedSource}; ContainsLocalDb={containsLocalDb}; Conn={masked}");

// Guard: fail fast in Production when still configured to LocalDB
if (builder.Environment.IsProduction() && containsLocalDb)
{
    throw new PlatformNotSupportedException("Application is running in Production but the configured DefaultConnection points to LocalDB which is unsupported on Azure. Set the connection string in Azure (Connection strings or App Settings) to your Azure SQL instance (ConnectionStrings__DefaultConnection) and restart.");
}

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddDbContext<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity - register UserManager/RoleManager so the async seeder can run from the API if desired
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<BordspelAppDbContext>()
    .AddDefaultTokenProviders();

// Add Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable authentication so Identity services are active during requests/startup
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

// Informative startup log about JSON settings
Console.WriteLine("JSON IgnoreCycles enabled ✅");

app.Run();
