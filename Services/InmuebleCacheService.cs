using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using StackExchange.Redis;



namespace PortalInmobiliario.Services
{
    public class InmuebleCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _mux;
        private readonly ApplicationDbContext _db;

        public InmuebleCacheService(IDistributedCache cache, IConnectionMultiplexer mux, ApplicationDbContext db)
        {
            _cache = cache;
            _mux = mux;
            _db = db;
        }

        public async Task<List<Inmueble>> GetInmueblesAsync(string ciudad, TipoInmueble? tipo, decimal? min, decimal? max, int? dormitorios)
        {
            var versionDb = _mux.GetDatabase();
            var version = await versionDb.StringGetAsync("Inmuebles:Version");
            var key = $"Inmuebles:List:{ciudad}:{tipo}:{min}:{max}:{dormitorios}:v{version}";

            var cached = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<List<Inmueble>>(cached);

            var query = _db.Inmuebles.Where(i => i.Activo);
            if (!string.IsNullOrEmpty(ciudad)) query = query.Where(i => i.Ciudad == ciudad);
            if (tipo.HasValue) query = query.Where(i => i.Tipo == tipo.Value);
            if (min.HasValue) query = query.Where(i => i.Precio >= min.Value);
            if (max.HasValue) query = query.Where(i => i.Precio <= max.Value);
            if (dormitorios.HasValue) query = query.Where(i => i.Dormitorios >= dormitorios.Value);

            var list = await query.ToListAsync();

            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(list), options);

            return list;
        }

        // invalidación simple: incrementar versión
        public async Task InvalidateCacheAsync()
        {
            var db = _mux.GetDatabase();
            await db.StringIncrementAsync("Inmuebles:Version");
        }
    }
}