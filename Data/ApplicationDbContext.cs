using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Models;

namespace portalinmobiliario1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Índice único para código
            builder.Entity<Inmueble>()
                .HasIndex(i => i.Codigo)
                .IsUnique();

            // Precisión decimal
            builder.Entity<Inmueble>()
                .Property(i => i.Precio)
                .HasPrecision(18, 2);

            builder.Entity<Inmueble>()
                .Property(i => i.MetrosCuadrados)
                .HasPrecision(18, 2);

            // Relaciones
            builder.Entity<Visita>()
                .HasOne(v => v.Inmueble)
                .WithMany(i => i.Visitas)
                .HasForeignKey(v => v.InmuebleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reserva>()
                .HasOne(r => r.Inmueble)
                .WithMany(i => i.Reservas)
                .HasForeignKey(r => r.InmuebleId)
                .OnDelete(DeleteBehavior.Cascade);

            // DATOS DE SEMILLA
            builder.Entity<Inmueble>().HasData(
                new Inmueble
                {
                    Id = 1,
                    Codigo = "DEP001",
                    Titulo = "Departamento Moderno Centro",
                    Tipo = TipoInmueble.Departamento,
                    Ciudad = "Lima",
                    Direccion = "Av. Arequipa 1234",
                    Dormitorios = 2,
                    Banos = 1,
                    MetrosCuadrados = 80m,
                    Precio = 150000m,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 2,
                    Codigo = "CASA001",
                    Titulo = "Casa Familiar San Borja",
                    Tipo = TipoInmueble.Casa,
                    Ciudad = "Lima",
                    Direccion = "Jr. Los Eucaliptos 567",
                    Dormitorios = 3,
                    Banos = 2,
                    MetrosCuadrados = 120m,
                    Precio = 280000m,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 3,
                    Codigo = "OF001",
                    Titulo = "Oficina Ejecutiva Miraflores",
                    Tipo = TipoInmueble.Oficina,
                    Ciudad = "Lima",
                    Direccion = "Av. Pardo 890",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 45m,
                    Precio = 95000m,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 4,
                    Codigo = "LOCAL001",
                    Titulo = "Local Comercial Av. Principal",
                    Tipo = TipoInmueble.Local,
                    Ciudad = "Lima",
                    Direccion = "Av. Javier Prado 1500",
                    Dormitorios = 0,
                    Banos = 2,
                    MetrosCuadrados = 200m,
                    Precio = 350000m,
                    Activo = true
                }
            );
        }
    }
}