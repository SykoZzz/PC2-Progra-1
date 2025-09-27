using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Models.ViewModels;


namespace PortalInmobiliario.Controllers
{
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public VisitasController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int inmuebleId)
        {
            ViewBag.InmuebleId = inmuebleId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int inmuebleId, DateTime fechaInicio, DateTime fechaFin, string notas)
        {
            if (fechaInicio >= fechaFin)
            {
                ModelState.AddModelError("", "La fecha de inicio debe ser menor a la fecha fin.");
            }

            // Horario laboral
            var startTime = fechaInicio.TimeOfDay;
            var endTime = fechaFin.TimeOfDay;
            if (startTime < TimeSpan.FromHours(8) || endTime > TimeSpan.FromHours(19))
            {
                ModelState.AddModelError("", "Las visitas deben estar entre 08:00 y 19:00.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.InmuebleId = inmuebleId;
                return View();
            }

            // Comprobar solapamiento con visitas no canceladas
            bool solapa = await _db.Visitas
                .Where(v => v.InmuebleId == inmuebleId && v.Estado != EstadoVisita.Cancelada)
                .AnyAsync(v => fechaInicio < v.FechaFin && v.FechaInicio < fechaFin);

            if (solapa)
            {
                ModelState.AddModelError("", "Ya existe una visita en ese intervalo para este inmueble.");
                ViewBag.InmuebleId = inmuebleId;
                return View();
            }

            var visita = new Visita
            {
                InmuebleId = inmuebleId,
                UsuarioId = _userManager.GetUserId(User),
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Notas = notas,
                Estado = EstadoVisita.Solicitada
            };

            _db.Visitas.Add(visita);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Visita solicitada correctamente.";
            return RedirectToAction("Detalle", "Catalogo", new { id = inmuebleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservarAhora(int inmuebleId)
        {
            var ahora = DateTime.UtcNow;
            bool existeActiva = await _db.Reservas.AnyAsync(r => r.InmuebleId == inmuebleId && r.FechaExpiracion > ahora);
            if (existeActiva)
            {
                TempData["Error"] = "El inmueble ya tiene una reserva activa.";
                return RedirectToAction("Detalle", "Catalogo", new { id = inmuebleId });
            }

            var reserva = new Reserva
            {
                InmuebleId = inmuebleId,
                UsuarioId = _userManager.GetUserId(User),
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddHours(48)
            };

            _db.Reservas.Add(reserva);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Reserva creada por 48 horas.";
            return RedirectToAction("Detalle", "Catalogo", new { id = inmuebleId });
        }
    }
}