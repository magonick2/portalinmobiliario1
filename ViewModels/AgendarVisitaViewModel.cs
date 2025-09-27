using System.ComponentModel.DataAnnotations;

namespace portalinmobiliario1.ViewModels
{
    public class AgendarVisitaViewModel
    {
        public int InmuebleId { get; set; }
        public string InmuebleTitulo { get; set; } = "";

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha y hora de inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddHours(9);

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [Display(Name = "Fecha y hora de fin")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddHours(10);

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        [Display(Name = "Notas adicionales")]
        public string? Notas { get; set; }

        // Validación personalizada
        public bool FechasValidas => FechaInicio < FechaFin;
        public bool EnHorarioLaboral =>
            FechaInicio.Hour >= 8 && FechaInicio.Hour < 19 &&
            FechaFin.Hour >= 8 && FechaFin.Hour <= 19;
    }
}

