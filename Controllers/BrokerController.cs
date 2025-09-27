using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Models;
using portalinmobiliario1.Services;
using Microsoft.AspNetCore.Identity;

namespace portalinmobiliario1.Controllers
{
    [Authorize(Roles = "Broker")] // Solo usuarios con rol Broker
    public class BrokerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly UserManager<IdentityUser> _userManager;

        public BrokerController(ApplicationDbContext context, ICacheService cacheService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _cacheService = cacheService;
            _userManager = userManager;
        }

        // Dashboard principal
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            // Visitas del día
            var visitasHoy = await _context.Visitas
                .Include(v => v.Inmueble)
                .Where(v => v.FechaInicio >= hoy && v.FechaInicio < manana)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();

            // Reservas activas
            var reservasActivas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Where(r => r.EstaActiva)
                .OrderBy(r => r.FechaExpiracion)
                .ToListAsync();

            // Estadísticas
            ViewBag.TotalInmuebles = await _context.Inmuebles.CountAsync();
            ViewBag.InmueblesActivos = await _context.Inmuebles.CountAsync(i => i.Activo);
            ViewBag.VisitasHoy = visitasHoy.Count;
            ViewBag.ReservasActivas = reservasActivas.Count;
            ViewBag.VisitasPendientes = await _context.Visitas.CountAsync(v => v.Estado == EstadoVisita.Solicitada);

            ViewBag.VisitasHoy = visitasHoy;
            ViewBag.ReservasActivas = reservasActivas;

            return View();
        }

        // CRUD Inmuebles
        public async Task<IActionResult> Inmuebles()
        {
            var inmuebles = await _context.Inmuebles
                .OrderByDescending(i => i.Id)
                .ToListAsync();
            return View(inmuebles);
        }

        public IActionResult CrearInmueble()
        {
            return View(new Inmueble());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInmueble(Inmueble inmueble)
        {
            // Validar código único
            if (await _context.Inmuebles.AnyAsync(i => i.Codigo == inmueble.Codigo))
            {
                ModelState.AddModelError(nameof(inmueble.Codigo), "El código ya existe");
            }

            if (ModelState.IsValid)
            {
                _context.Inmuebles.Add(inmueble);
                await _context.SaveChangesAsync();

                // Invalidar caché
                await _cacheService.RemoveByPatternAsync("inmuebles");

                TempData["Success"] = "Inmueble creado correctamente";
                return RedirectToAction(nameof(Inmuebles));
            }

            return View(inmueble);
        }

        public async Task<IActionResult> EditarInmueble(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarInmueble(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id)
                return NotFound();

            // Validar código único (excluyendo el actual)
            if (await _context.Inmuebles.AnyAsync(i => i.Codigo == inmueble.Codigo && i.Id != id))
            {
                ModelState.AddModelError(nameof(inmueble.Codigo), "El código ya existe");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();

                    // Invalidar caché
                    await _cacheService.RemoveByPatternAsync("inmuebles");

                    TempData["Success"] = "Inmueble actualizado correctamente";
                    return RedirectToAction(nameof(Inmuebles));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Inmuebles.AnyAsync(i => i.Id == id))
                        return NotFound();
                    throw;
                }
            }

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
                return NotFound();

            inmueble.Activo = !inmueble.Activo;
            await _context.SaveChangesAsync();

            // Invalidar caché
            await _cacheService.RemoveByPatternAsync("inmuebles");

            var estado = inmueble.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Inmueble {estado} correctamente";

            return RedirectToAction(nameof(Inmuebles));
        }

        // Gestión de Visitas
        public async Task<IActionResult> Visitas()
        {
            var visitas = await _context.Visitas
                .Include(v => v.Inmueble)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();

            return View(visitas);
        }

        public async Task<IActionResult> AgendaDelDia()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var visitasHoy = await _context.Visitas
                .Include(v => v.Inmueble)
                .Where(v => v.FechaInicio >= hoy && v.FechaInicio < manana)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();

            return View(visitasHoy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                return NotFound();

            visita.Estado = EstadoVisita.Confirmada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Visita confirmada correctamente";
            return RedirectToAction(nameof(Visitas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
                return NotFound();

            visita.Estado = EstadoVisita.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Visita cancelada correctamente";
            return RedirectToAction(nameof(Visitas));
        }

        // Gestión de Reservas
        public async Task<IActionResult> Reservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Inmueble)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound();

            // Cambiar fecha de expiración al pasado para "liberar" la reserva
            reserva.FechaExpiracion = DateTime.Now.AddMinutes(-1);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva liberada correctamente";
            return RedirectToAction(nameof(Reservas));
        }

        // Crear usuario Broker
        public async Task<IActionResult> CrearBroker()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearBroker(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Email y contraseña son requeridos";
                return View();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Broker");
                TempData["Success"] = $"Usuario Broker {email} creado correctamente";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
    }
}
