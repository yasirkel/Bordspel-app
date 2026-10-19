using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BordspelAppDbContext>();
    context.Database.Migrate();
    DbSeeder.Seed(context);

    // Development-only: repair existing AspNetUsers.PersonId by matching Persons.Email
    var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Startup");
    if (env.IsDevelopment())
    {
        try
        {
            logger.LogInformation("Development DB repair: backfilling AspNetUsers.PersonId from Persons by email.");
            var sql = @"UPDATE u SET PersonId = p.Id FROM AspNetUsers u JOIN Persons p ON p.Email = u.Email WHERE u.PersonId IS NULL;";
            context.Database.ExecuteSqlRaw(sql);
            logger.LogInformation("DB repair executed.");
        }
        catch (System.Exception ex)
        {
            logger.LogWarning(ex, "Development DB repair failed.");
        }
    }

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
