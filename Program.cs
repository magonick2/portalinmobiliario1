using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Services;

var builder = WebApplication.CreateBuilder(args);

// Database - Simple
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "DataSource=app.db;Cache=Shared";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity básico
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Cache simple
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// Session básica
builder.Services.AddSession();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline simple
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();