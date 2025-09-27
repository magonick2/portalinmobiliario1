var builder = WebApplication.CreateBuilder(args);

// Configuración mínima
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline mínimo
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();