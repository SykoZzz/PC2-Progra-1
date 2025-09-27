using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;


namespace PortalInmobiliario.Areas.Broker.Controllers
{
[Area("Broker")]
[Authorize(Roles = "Broker")]
public class AgendaController : Controller
{
    private readonly ApplicationDbContext _db;
    public AgendaController(ApplicationDbContext db) { _db = db; }

    public async Task<IActionResult> Index(DateTime? dia)
    {
        var fecha = dia?.Date ?? DateTime.UtcNow.Date;
        var visitas = await _db.Visitas
            .Include(v => v.Inmueble)
            .Where(v => v.FechaInicio.Date == fecha)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();

        return View(visitas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        var v = await _db.Visitas.FindAsync(id);
        v.Estado = EstadoVisita.Confirmada;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var v = await _db.Visitas.FindAsync(id);
        v.Estado = EstadoVisita.Cancelada;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

}