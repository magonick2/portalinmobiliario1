using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace portalinmobiliario1.Models
{
    public enum EstadoVisita
    {
        Solicitada,
        Confirmada,
        Cancelada
    }

    public class Visita
    {
        public int Id { get; set; }

        public int InmuebleId { get; set; }
        public Inmueble Inmueble { get; set; } = null!;

        [Required]
        public string UsuarioId { get; set; } = "";
        public IdentityUser Usuario { get; set; } = null!;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public EstadoVisita Estado { get; set; } = EstadoVisita.Solicitada;

        [StringLength(500)]
        public string? Notas { get; set; }
    }
}
