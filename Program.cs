using Bordspel_app.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bordspel_app.Models;

var builder = WebApplication.CreateBuilder(args);

var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
		   ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
		   ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection");
var rawConn = !string.IsNullOrWhiteSpace(envConn) ? envConn : cfgConn ?? string.Empty;
var containsLocalDb = rawConn.IndexOf("(localdb)", StringComparison.OrdinalIgnoreCase) >= 0 || rawConn.IndexOf("mssqllocaldb", StringComparison.OrdinalIgnoreCase) >= 0;

if (builder.Environment.IsProduction() && containsLocalDb)
{
	throw new PlatformNotSupportedException("Application is running in Production but the configured DefaultConnection points to LocalDB which is unsupported on Azure. Set the connection string in Azure (Connection strings or App Settings) to your Azure SQL instance (ConnectionStrings__DefaultConnection) and restart.");
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Add Razor Pages services

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Make sure this is added before authorization
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for Identity
app.MapRazorPages(); // Add this to map Identity Razor Pages

app.Run();
