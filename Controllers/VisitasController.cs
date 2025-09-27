using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Models;
using portalinmobiliario1.ViewModels;
using System.Security.Claims;

namespace portalinmobiliario1.Controllers
{
    [Authorize] // Requiere usuario autenticado
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Agendar(int inmuebleId)
        {
            var inmueble = await _context.Inmuebles.FindAsync(inmuebleId);
            if (inmueble == null || !inmueble.Activo)
            {
                TempData["Error"] = "Inmueble no encontrado o no disponible";
                return RedirectToAction("Index", "Catalogo");
            }

            var model = new AgendarVisitaViewModel
            {
                InmuebleId = inmuebleId,
                InmuebleTitulo = inmueble.Titulo,
                FechaInicio = DateTime.Today.AddDays(1).AddHours(9), // Mañana a las 9 AM
                FechaFin = DateTime.Today.AddDays(1).AddHours(10)    // Mañana a las 10 AM
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarVisitaViewModel model)
        {
            // Recargar título para mostrar en caso de error
            var inmueble = await _context.Inmuebles.FindAsync(model.InmuebleId);
            if (inmueble != null)
                model.InmuebleTitulo = inmueble.Titulo;

            // Validaciones personalizadas
            if (!model.FechasValidas)
            {
                ModelState.AddModelError(nameof(model.FechaFin),
                    "La fecha de fin debe ser posterior a la fecha de inicio");
            }

            if (!model.EnHorarioLaboral)
            {
                ModelState.AddModelError("",
                    "Las visitas solo pueden agendarse en horario laboral (8:00 - 19:00)");
            }

            // Verificar que no haya visitas solapadas
            var visitasSolapadas = await _context.Visitas
                .Where(v => v.InmuebleId == model.InmuebleId &&
                           v.Estado != EstadoVisita.Cancelada &&
                           ((model.FechaInicio >= v.FechaInicio && model.FechaInicio < v.FechaFin) ||
                            (model.FechaFin > v.FechaInicio && model.FechaFin <= v.FechaFin) ||
                            (model.FechaInicio <= v.FechaInicio && model.FechaFin >= v.FechaFin)))
                .AnyAsync();

            if (visitasSolapadas)
            {
                ModelState.AddModelError("",
                    "Ya existe una visita programada en ese horario para este inmueble");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Crear la visita
            var visita = new Visita
            {
                InmuebleId = model.InmuebleId,
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Notas = model.Notas,
                Estado = EstadoVisita.Solicitada
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Visita agendada correctamente. Pronto recibirás confirmación.";
            return RedirectToAction("Detalle", "Catalogo", new { id = model.InmuebleId });
        }

        public async Task<IActionResult> MisVisitas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visitas = await _context.Visitas
                .Include(v => v.Inmueble)
                .Where(v => v.UsuarioId == userId)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();

            return View(visitas);
        }
    }
}