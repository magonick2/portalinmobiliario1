using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Models;
using portalinmobiliario1.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = "Production";
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
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"));

var app = builder.Build();

// Pipeline simple
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
// Asegurar que la base de datos y datos existen
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Si no hay inmuebles, agregar datos de prueba
    if (!context.Inmuebles.Any())
    {
        context.Inmuebles.AddRange(
            new Inmueble { Codigo = "DEP001", Titulo = "Departamento Centro", Tipo = TipoInmueble.Departamento, Ciudad = "Lima", Direccion = "Av. Arequipa 123", Dormitorios = 2, Banos = 1, MetrosCuadrados = 80, Precio = 150000, Activo = true },
            new Inmueble { Codigo = "CASA001", Titulo = "Casa Familiar", Tipo = TipoInmueble.Casa, Ciudad = "Lima", Direccion = "Jr. Los Olivos 456", Dormitorios = 3, Banos = 2, MetrosCuadrados = 120, Precio = 280000, Activo = true }
        );
        context.SaveChanges();
    }
}


app.Run();