using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portalinmobiliario1.Data;
using portalinmobiliario1.Extensions;
using portalinmobiliario1.Models;
using portalinmobiliario1.Services;
using portalinmobiliario1.ViewModels;

namespace portalinmobiliario1.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private const int TamanoPagina = 6;

        public CatalogoController(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index(CatalogoViewModel? model)
        {
            // Si no se proporcionan filtros, recuperar de sesión
            if (model == null || (string.IsNullOrEmpty(model.Ciudad) && !model.Tipo.HasValue &&
                !model.PrecioMin.HasValue && !model.PrecioMax.HasValue && !model.Dormitorios.HasValue))
            {
                var filtrosSesion = HttpContext.Session.GetObject<CatalogoViewModel>("UltimosFiltros");
                if (filtrosSesion != null)
                {
                    model = filtrosSesion;
                    ViewBag.FiltrosRecuperados = true;
                }
                else
                {
                    model = new CatalogoViewModel();
                }
            }

            // Guardar filtros actuales en sesión
            HttpContext.Session.SetObject("UltimosFiltros", model);

            // Validaciones
            if (model.TieneErrorRangoPrecios)
            {
                ModelState.AddModelError("", "El precio mínimo no puede ser mayor al precio máximo");
            }

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

            // Crear clave de caché basada en filtros
            var cacheKey = $"inmuebles_{model.Ciudad}_{model.Tipo}_{model.PrecioMin}_{model.PrecioMax}_{model.Dormitorios}_{model.Pagina}";

            // Intentar obtener del caché
            var resultadoCache = await _cacheService.GetAsync<CatalogoViewModel>(cacheKey);
            if (resultadoCache != null && ModelState.IsValid)
            {
                ViewBag.DesdCache = true;
                return View(resultadoCache);
            }

            // Si no está en caché, consultar base de datos
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

            // Paginación
            var totalInmuebles = await query.CountAsync();
            model.TotalPaginas = (int)Math.Ceiling(totalInmuebles / (double)TamanoPagina);

            if (model.Pagina < 1)
                model.Pagina = 1;
            if (model.Pagina > model.TotalPaginas && model.TotalPaginas > 0)
                model.Pagina = model.TotalPaginas;

            var inmuebles = await query
                .OrderBy(i => (double)i.Precio)
                .Skip((model.Pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();

            model.Inmuebles = inmuebles;

            // Guardar en caché por 60 segundos solo si la consulta es válida
            if (ModelState.IsValid)
            {
                await _cacheService.SetAsync(cacheKey, model, TimeSpan.FromSeconds(60));
            }

            return View(model);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles
                .Include(i => i.Reservas)
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inmueble == null)
            {
                TempData["Error"] = "Inmueble no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Guardar último inmueble visitado en sesión
            HttpContext.Session.SetInt32("UltimoInmuebleId", id);
            HttpContext.Session.SetString("UltimoInmuebleTitulo", inmueble.Titulo);

            return View(inmueble);
        }

        // Método para invalidar caché (para uso del panel Broker)
        public async Task<IActionResult> InvalidarCache()
        {
            await _cacheService.RemoveByPatternAsync("inmuebles");
            TempData["Success"] = "Caché invalidado correctamente";
            return RedirectToAction(nameof(Index));
        }
    }
}
