using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var domainConnection = builder.Configuration.GetConnectionString("DomainConnection") ?? string.Empty;
var identityConnection = builder.Configuration.GetConnectionString("IdentityConnection") ?? string.Empty;
var containsLocalDb = domainConnection.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
    || domainConnection.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase)
    || identityConnection.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
    || identityConnection.Contains("mssqllocaldb", StringComparison.OrdinalIgnoreCase);

if (builder.Environment.IsProduction() && containsLocalDb)
{
    throw new PlatformNotSupportedException("Application is running in Production but one of the configured database connections points to LocalDB. Configure separate Azure SQL connection strings for DomainConnection and IdentityConnection.");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind Api options from configuration (appsettings / env vars)
builder.Services.Configure<BordspelApp.WebApp.Configuration.ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<BordspelApp.WebApp.Services.IApiBaseUrlProvider, BordspelApp.WebApp.Services.ApiBaseUrlProvider>();

// Add DbContext
builder.Services.AddDbContext<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DomainConnection")));

builder.Services.AddDbContext<BordspelAppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<BordspelAppIdentityDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();
builder.Services.AddScoped<IPotluckRepository, PotluckRepository>();

// Services
builder.Services.AddScoped<IGameNightService, BordspelApp.Infrastructure.Services.GameNightService>();
builder.Services.AddScoped<IRegistrationService, BordspelApp.Infrastructure.Services.RegistrationService>();
builder.Services.AddScoped<IBoardGameService, BordspelApp.Infrastructure.Services.BoardGameService>();
builder.Services.AddScoped<IPersonService, BordspelApp.Infrastructure.Services.PersonService>();
builder.Services.AddScoped<IPotluckService, BordspelApp.Infrastructure.Services.PotluckService>();

// HTTP client for calling API; base address is provided by ApiBaseUrlProvider
builder.Services.AddHttpClient("api").ConfigureHttpClient((sp, client) =>
{
    var provider = sp.GetRequiredService<BordspelApp.WebApp.Services.IApiBaseUrlProvider>();
    var baseUrl = provider.GetBaseUrl();
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BordspelAppDbContext>();
    DbSeeder.Seed(context);

    var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Startup");

    // Optionally run the async DatabaseSeeder (creates Identity users and roles) when requested.
    var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
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

    // If user requested seeding only, exit without starting the web host
    var seedOnly = config.GetValue<bool>("SeedDatabaseOnly") || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SeedDatabaseOnly"));
    if (seedOnly)
    {
        logger.LogInformation("SeedDatabaseOnly set - exiting after seeding.");
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
