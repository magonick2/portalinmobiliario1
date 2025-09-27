using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace portalinmobiliario1.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public int InmuebleId { get; set; }
        public Inmueble Inmueble { get; set; } = null!;

        [Required]
        public string UsuarioId { get; set; } = "";
        public IdentityUser Usuario { get; set; } = null!;

        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool EstaActiva => DateTime.Now < FechaExpiracion;
    }
}

