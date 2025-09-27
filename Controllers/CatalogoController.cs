using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Models.ViewModels;
using PortalInmobiliario.Services; // 👈 Asegúrate de tener este using
using Microsoft.EntityFrameworkCore;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly InmuebleCacheService _inmuebleCacheService;

        public CatalogoController(InmuebleCacheService inmuebleCacheService)
        {
            _inmuebleCacheService = inmuebleCacheService;
        }

        public async Task<IActionResult> Index(string ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1)
        {
            // Validación server-side
            if (precioMin < 0 || precioMax < 0 || dormitorios < 0) 
                ModelState.AddModelError("", "Los parámetros numéricos no pueden ser negativos.");
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax) 
                ModelState.AddModelError("", "El precio mínimo no puede ser mayor que el máximo.");

            if (!ModelState.IsValid)
            {
                var vmErr = new CatalogoViewModel 
                { 
                    Inmuebles = Enumerable.Empty<Inmueble>(), 
                    Page = 1, 
                    PageSize = 10, 
                    Total = 0 
                };
                return View(vmErr);
            }

            // ✅ Ahora obtenemos desde cache en vez de _db
            var inmuebles = await _inmuebleCacheService.GetInmueblesAsync(
                                ciudad, tipo, precioMin, precioMax, dormitorios);

            const int pageSize = 10;
            var total = inmuebles.Count();
            var items = inmuebles.OrderBy(i => i.Titulo)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            // Guardar filtros en sesión
            HttpContext.Session.SetString("LastCatalogFilters", 
                JsonSerializer.Serialize(new { ciudad, tipo, precioMin, precioMax, dormitorios }));

            var vm = new CatalogoViewModel 
            { 
                Inmuebles = items, 
                Page = page, 
                PageSize = pageSize, 
                Total = total, 
                Ciudad = ciudad, 
                Tipo = tipo, 
                PrecioMin = precioMin, 
                PrecioMax = precioMax, 
                Dormitorios = dormitorios 
            };

            return View(vm);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            // Aquí aún puedes usar el DbContext porque es un detalle puntual
            using var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var inmueble = await db.Inmuebles.Include(i => i.Reservas).FirstOrDefaultAsync(i => i.Id == id);
            if (inmueble == null) return NotFound();

            HttpContext.Session.SetInt32("LastInmuebleId", id);
            HttpContext.Session.SetString("LastInmuebleTitulo", inmueble.Titulo);

            return View(inmueble);
        }
    }
}
