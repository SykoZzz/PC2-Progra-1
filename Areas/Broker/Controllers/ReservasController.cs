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
public class ReservasController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReservasController(ApplicationDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        var ahora = DateTime.UtcNow;
        var reservas = await _db.Reservas.Include(r => r.Inmueble)
                          .Where(r => r.FechaExpiracion > ahora)
                          .OrderBy(r => r.FechaExpiracion)
                          .ToListAsync();
        return View(reservas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar(int id)
    {
        var r = await _db.Reservas.FindAsync(id);
        if (r != null)
        {
            r.FechaExpiracion = DateTime.UtcNow; // marcar como expirada
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

}