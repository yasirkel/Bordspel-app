using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
           ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection");
var usedSource = !string.IsNullOrWhiteSpace(envConn) ? "env var (ConnectionStrings__DefaultConnection/SQLAZURECONNSTR_*)" : (string.IsNullOrWhiteSpace(cfgConn) ? "none" : "config (ConnectionStrings:DefaultConnection)");
var rawConn = !string.IsNullOrWhiteSpace(envConn) ? envConn : cfgConn ?? string.Empty;
var masked = MaskConnectionString(rawConn);
var containsLocalDb = rawConn.IndexOf("(localdb)", StringComparison.OrdinalIgnoreCase) >= 0 || rawConn.IndexOf("mssqllocaldb", StringComparison.OrdinalIgnoreCase) >= 0;
Console.WriteLine($"STARTUP: ENV={_envName}; Source={usedSource}; ContainsLocalDb={containsLocalDb}; Conn={masked}");

if (builder.Environment.IsProduction() && containsLocalDb)
{
    throw new PlatformNotSupportedException("Application is running in Production but the configured DefaultConnection points to LocalDB which is unsupported on Azure. Set the connection string in Azure (Connection strings or App Settings) to your Azure SQL instance (ConnectionStrings__DefaultConnection) and restart.");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind Api options from configuration (appsettings / env vars)
builder.Services.Configure<BordspelApp.WebApp.Configuration.ApiOptions>(builder.Configuration.GetSection("Api"));

builder.Services.AddSingleton<BordspelApp.WebApp.Services.IApiBaseUrlProvider, BordspelApp.WebApp.Services.ApiBaseUrlProvider>();

// Add DbContext
builder.Services.AddDbContext<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<BordspelAppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

// Services
builder.Services.AddScoped<IGameNightService, BordspelApp.Infrastructure.Services.GameNightService>();
builder.Services.AddScoped<IRegistrationService, BordspelApp.Infrastructure.Services.RegistrationService>();
builder.Services.AddScoped<IBoardGameService, BordspelApp.Infrastructure.Services.BoardGameService>();
builder.Services.AddScoped<IPersonService, BordspelApp.Infrastructure.Services.PersonService>();

// HTTP client for calling API; base address is provided by ApiBaseUrlProvider
builder.Services.AddHttpClient("api").ConfigureHttpClient((sp, client) =>
{
    var provider = sp.GetRequiredService<BordspelApp.WebApp.Services.IApiBaseUrlProvider>();
    var baseUrl = provider.GetBaseUrl();
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

//
// ✅ DB setup policy:
// - Development: migrate + seed ok
// - Production (Azure): DO NOT migrate/seed by default (prevents LocalDB/connection issues and startup crash)
// - If you ever want to run it in Production ON PURPOSE: set App Setting RUN_DB_SETUP=true
//
var runDbSetup =
    app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("RUN_DB_SETUP") ||
    string.Equals(Environment.GetEnvironmentVariable("RUN_DB_SETUP"), "true", StringComparison.OrdinalIgnoreCase);

if (runDbSetup)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BordspelAppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Startup");
    var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
    var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

    try
    {
        // Migrate + seed
        context.Database.Migrate();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "DB migrate/seed failed.");
        throw; // In dev you want a hard fail; in prod keep RUN_DB_SETUP off
    }

    // Development-only: repair existing AspNetUsers.PersonId by matching Persons.Email
    if (env.IsDevelopment())
    {
        try
        {
            logger.LogInformation("Development DB repair: backfilling AspNetUsers.PersonId from Persons by email.");
            var sql = @"UPDATE u SET PersonId = p.Id
                        FROM AspNetUsers u
                        JOIN Persons p ON p.Email = u.Email
                        WHERE u.PersonId IS NULL;";
            context.Database.ExecuteSqlRaw(sql);
            logger.LogInformation("DB repair executed.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Development DB repair failed.");
        }
    }

    // Optionally run the async DatabaseSeeder (creates Identity users and roles) when requested.
    // NOTE: Keep it dev-only by default; enable with SeedDatabase=true (or RUN_DB_SETUP=true already)
    var runAsyncSeeder = env.IsDevelopment() || config.GetValue<bool>("SeedDatabase");
    if (runAsyncSeeder)
    {
        logger.LogInformation("Running async DatabaseSeeder.SeedAsync (Development or SeedDatabase=true).");
        try
        {
            BordspelApp.Infrastructure.Data.DatabaseSeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            logger.LogInformation("Async DatabaseSeeder completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Async DatabaseSeeder failed.");
            throw;
        }
    }

    // SeedDatabaseOnly is dangerous in Production because it will stop the web host.
    // Only respect it in Development.
    var seedOnlyRequested =
        config.GetValue<bool>("SeedDatabaseOnly") ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SeedDatabaseOnly"));

    if (env.IsDevelopment() && seedOnlyRequested)
    {
        logger.LogInformation("SeedDatabaseOnly set (Development) - exiting after seeding.");
        return;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
