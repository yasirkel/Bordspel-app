using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Identity;
using BordspelApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
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

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BordspelAppDbContext>();
    context.Database.Migrate();
    DbSeeder.Seed(context);
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
