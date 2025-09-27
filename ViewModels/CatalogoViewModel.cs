using portalinmobiliario1.Models;
using System.ComponentModel.DataAnnotations;

namespace portalinmobiliario1.ViewModels
{
    public class CatalogoViewModel
    {
        public string? Ciudad { get; set; }
        public TipoInmueble? Tipo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo debe ser mayor o igual a 0")]
        public decimal? PrecioMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo debe ser mayor o igual a 0")]
        public decimal? PrecioMax { get; set; }

        [Range(0, 10, ErrorMessage = "Los dormitorios deben estar entre 0 y 10")]
        public int? Dormitorios { get; set; }

        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public List<Inmueble> Inmuebles { get; set; } = new();

        // Validación para rango de precios
        public bool TieneErrorRangoPrecios => PrecioMin.HasValue && PrecioMax.HasValue && PrecioMin > PrecioMax;
    }
}