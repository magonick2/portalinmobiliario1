using System.ComponentModel.DataAnnotations;

namespace portalinmobiliario1.Models
{
    public enum TipoInmueble
    {
        Departamento,
        Casa,
        Oficina,
        Local
    }

    public class Inmueble
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = "";

        public string? Imagen { get; set; }

        public TipoInmueble Tipo { get; set; }

        [Required]
        [StringLength(100)]
        public string Ciudad { get; set; } = "";

        [Required]
        [StringLength(300)]
        public string Direccion { get; set; } = "";

        [Range(1, 10)]
        public int Dormitorios { get; set; }

        [Range(1, 10)]
        public int Banos { get; set; }

        [Range(1, 1000)]
        public decimal MetrosCuadrados { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        // Navigation properties
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
