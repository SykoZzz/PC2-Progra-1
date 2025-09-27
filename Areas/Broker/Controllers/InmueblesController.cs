using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services;


namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly InmuebleCacheService _cacheService;

        public InmueblesController(ApplicationDbContext db, InmuebleCacheService cacheService)
        {
            _db = db;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _db.Inmuebles.OrderBy(i => i.Titulo).ToListAsync();
            return View(items);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Inmuebles.Add(model);
            await _db.SaveChangesAsync();
            await _cacheService.InvalidateCacheAsync(); // invalidar cache
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _db.Inmuebles.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inmueble model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Inmuebles.Update(model);
            await _db.SaveChangesAsync();
            await _cacheService.InvalidateCacheAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _db.Inmuebles.FindAsync(id);
            item.Activo = !item.Activo;
            await _db.SaveChangesAsync();
            await _cacheService.InvalidateCacheAsync();
            return RedirectToAction(nameof(Index));
        }
    }

}