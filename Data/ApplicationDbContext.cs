using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{

    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {


        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public DbSet<Inmueble> Inmuebles { get; set; }
            public DbSet<Visita> Visitas { get; set; }
            public DbSet<Reserva> Reservas { get; set; }

            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options) { }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<Inmueble>()
                    .HasIndex(i => i.Codigo)
                    .IsUnique();

                // Check constraints (precio y metros > 0)
                builder.Entity<Inmueble>()
                    .ToTable(t => t.HasCheckConstraint("CK_Inmueble_Precio_Metros", "Precio > 0 AND MetrosCuadrados > 0"));

                // Relación
                builder.Entity<Visita>()
                    .HasOne(v => v.Inmueble)
                    .WithMany(i => i.Visitas)
                    .HasForeignKey(v => v.InmuebleId);

                builder.Entity<Reserva>()
                    .HasOne(r => r.Inmueble)
                    .WithMany(i => i.Reservas)
                    .HasForeignKey(r => r.InmuebleId);

                // Seed mínima
                builder.Entity<Inmueble>().HasData(
                    new Inmueble { Id = 1, Codigo = "A101", Titulo = "Depto céntrico 2D", Tipo = TipoInmueble.Departamento, Ciudad = "Lima", Direccion = "Av. Principal 123", Dormitorios = 2, Banos = 1, MetrosCuadrados = 60, Precio = 120000, Activo = true, Imagen = "imagen1.jpg" },
                    new Inmueble { Id = 2, Codigo = "C201", Titulo = "Casa con jardín", Tipo = TipoInmueble.Casa, Ciudad = "Lima", Direccion = "Calle Falsa 456", Dormitorios = 3, Banos = 2, MetrosCuadrados = 150, Precio = 250000, Activo = true, Imagen = "imagen2.jpg" },
                    new Inmueble { Id = 3, Codigo = "O301", Titulo = "Oficina moderna", Tipo = TipoInmueble.Oficina, Ciudad = "San Isidro", Direccion = "Paseo 10", Dormitorios = 0, Banos = 1, MetrosCuadrados = 80, Precio = 90000, Activo = true, Imagen = "imagen3.jpg" }
                );
            }
        }
    }
}