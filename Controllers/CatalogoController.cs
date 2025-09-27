using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Models;
using portalinmobiliario1.ViewModels;

namespace portalinmobiliario1.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int TamanoPagina = 6; // 6 inmuebles por página

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CatalogoViewModel model)
        {
            // Validación del rango de precios
            if (model.TieneErrorRangoPrecios)
            {
                ModelState.AddModelError("", "El precio mínimo no puede ser mayor al precio máximo");
            }

            // Validar parámetros numéricos negativos
            if (model.PrecioMin < 0)
            {
                ModelState.AddModelError(nameof(model.PrecioMin), "El precio mínimo no puede ser negativo");
            }

            if (model.PrecioMax < 0)
            {
                ModelState.AddModelError(nameof(model.PrecioMax), "El precio máximo no puede ser negativo");
            }

            if (model.Dormitorios < 0)
            {
                ModelState.AddModelError(nameof(model.Dormitorios), "Los dormitorios no pueden ser negativos");
            }

            // Consulta base: solo inmuebles activos
            var query = _context.Inmuebles.Where(i => i.Activo);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(model.Ciudad))
            {
                query = query.Where(i => i.Ciudad.ToLower().Contains(model.Ciudad.ToLower()));
            }

            if (model.Tipo.HasValue)
            {
                query = query.Where(i => i.Tipo == model.Tipo.Value);
            }

            if (model.PrecioMin.HasValue)
            {
                query = query.Where(i => i.Precio >= model.PrecioMin.Value);
            }

            if (model.PrecioMax.HasValue)
            {
                query = query.Where(i => i.Precio <= model.PrecioMax.Value);
            }

            if (model.Dormitorios.HasValue)
            {
                query = query.Where(i => i.Dormitorios >= model.Dormitorios.Value);
            }

            // Calcular paginación
            var totalInmuebles = await query.CountAsync();
            model.TotalPaginas = (int)Math.Ceiling(totalInmuebles / (double)TamanoPagina);

            // Validar página actual
            if (model.Pagina < 1)
                model.Pagina = 1;
            if (model.Pagina > model.TotalPaginas && model.TotalPaginas > 0)
                model.Pagina = model.TotalPaginas;

            // Obtener inmuebles con paginación
            var inmuebles = await query
                .OrderBy(i => (double)i.Precio)
                .Skip((model.Pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();

            model.Inmuebles = inmuebles;

            return View(model);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inmueble == null)
            {
                TempData["Error"] = "Inmueble no encontrado";
                return RedirectToAction(nameof(Index));
            }

            return View(inmueble);
        }
    }
}