using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Models;
using System.Security.Claims;

namespace portalinmobiliario1.Controllers
{
    [Authorize] // Requiere usuario autenticado
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int inmuebleId)
        {
            var inmueble = await _context.Inmuebles.FindAsync(inmuebleId);
            if (inmueble == null || !inmueble.Activo)
            {
                TempData["Error"] = "Inmueble no encontrado o no disponible";
                return RedirectToAction("Index", "Catalogo");
            }

            // Verificar si ya existe una reserva activa para este inmueble
            var reservaExistente = await _context.Reservas
                .Where(r => r.InmuebleId == inmuebleId &&
                           DateTime.Now < r.FechaExpiracion)
                .AnyAsync();

            if (reservaExistente)
            {
                TempData["Error"] = "Este inmueble ya tiene una reserva activa";
                return RedirectToAction("Detalle", "Catalogo", new { id = inmuebleId });
            }

            // Crear la reserva por 48 horas
            var reserva = new Reserva
            {
                InmuebleId = inmuebleId,
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                FechaExpiracion = DateTime.Now.AddHours(48),
                FechaCreacion = DateTime.Now
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inmueble reservado por 48 horas hasta el {reserva.FechaExpiracion:dd/MM/yyyy HH:mm}";
            return RedirectToAction("Detalle", "Catalogo", new { id = inmuebleId });
        }

        public async Task<IActionResult> MisReservas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Where(r => r.UsuarioId == userId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View(reservas);
        }
    }
}