using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Inmueble> Inmuebles { get; set; }
    public DbSet<Visita> Visitas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Inmueble>()
            .HasIndex(i => i.Codigo)
            .IsUnique();

        // Check constraint simple: precio > 0 and metros > 0
        builder.Entity<Inmueble>()
            .HasCheckConstraint("CK_Inmueble_Precio_Metros", "Precio > 0 AND MetrosCuadrados > 0");

        builder.Entity<Visita>()
            .HasOne(v => v.Inmueble)
            .WithMany(i => i.Visitas)
            .HasForeignKey(v => v.InmuebleId);

        builder.Entity<Reserva>()
            .HasOne(r => r.Inmueble)
            .WithMany(i => i.Reservas)
            .HasForeignKey(r => r.InmuebleId);

        // Seed mínimo
        builder.Entity<Inmueble>().HasData(
            new Inmueble { Id = 1, Codigo = "A101", Titulo = "Depto céntrico 2D", Imagen = "https://images.unsplash.com/photo-1502672023488-70e25813eb80",Tipo = TipoInmueble.Departamento, Ciudad = "Lima", Direccion = "Av. Principal 123", Dormitorios = 2, Banos = 1, MetrosCuadrados = 60, Precio = 120000m, Activo = true },
            new Inmueble { Id = 2, Codigo = "C201", Titulo = "Casa con jardín", Imagen = "https://images.unsplash.com/photo-1568605114967-8130f3a36994", Tipo = TipoInmueble.Casa, Ciudad = "Lima", Direccion = "Calle Falsa 456", Dormitorios = 3, Banos = 2, MetrosCuadrados = 150, Precio = 250000m, Activo = true },
            new Inmueble { Id = 3, Codigo = "O301", Titulo = "Oficina moderna", Imagen = "https://images.unsplash.com/photo-1507679799987-c73779587ccf",Tipo = TipoInmueble.Oficina, Ciudad = "San Isidro", Direccion = "Paseo 10", Dormitorios = 0, Banos = 1, MetrosCuadrados = 80, Precio = 90000m, Activo = true }
        );
    }
}
}