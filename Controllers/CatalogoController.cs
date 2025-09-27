using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Models.ViewModels;

namespace PortalInmobiliario.Controllers
{
public class CatalogoController : Controller
{
    private readonly ApplicationDbContext _db;
    public CatalogoController(ApplicationDbContext db) { _db = db; }

    public async Task<IActionResult> Index(string ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1)
    {
        // Validación server-side
        if (precioMin < 0 || precioMax < 0 || dormitorios < 0) ModelState.AddModelError("", "Los parámetros numéricos no pueden ser negativos.");
        if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax) ModelState.AddModelError("", "El precio mínimo no puede ser mayor que el máximo.");

        if (!ModelState.IsValid)
        {
            var vmErr = new CatalogoViewModel { Inmuebles = Enumerable.Empty<Inmueble>(), Page = 1, PageSize = 10, Total = 0 };
            return View(vmErr);
        }

        var query = _db.Inmuebles.Where(i => i.Activo);

        if (!string.IsNullOrWhiteSpace(ciudad)) query = query.Where(i => i.Ciudad == ciudad);
        if (tipo.HasValue) query = query.Where(i => i.Tipo == tipo.Value);
        if (precioMin.HasValue) query = query.Where(i => i.Precio >= precioMin.Value);
        if (precioMax.HasValue) query = query.Where(i => i.Precio <= precioMax.Value);
        if (dormitorios.HasValue) query = query.Where(i => i.Dormitorios >= dormitorios.Value);

        const int pageSize = 10;
        var total = await query.CountAsync();
        var items = await query.OrderBy(i => i.Titulo)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        HttpContext.Session.SetString("LastCatalogFilters", JsonSerializer.Serialize(new { ciudad, tipo, precioMin, precioMax, dormitorios }));

        var vm = new CatalogoViewModel { Inmuebles = items, Page = page, PageSize = pageSize, Total = total, Ciudad = ciudad, Tipo = tipo, PrecioMin = precioMin, PrecioMax = precioMax, Dormitorios = dormitorios };
        return View(vm);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var inmueble = await _db.Inmuebles.Include(i => i.Reservas).FirstOrDefaultAsync(i => i.Id == id);
        if (inmueble == null) return NotFound();

        HttpContext.Session.SetInt32("LastInmuebleId", id);

        return View(inmueble);
    }
}
}