using System.ComponentModel.DataAnnotations;


namespace PortalInmobiliario.Models
{
public enum TipoInmueble { Departamento, Casa, Oficina, Local }

public class Inmueble
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string Codigo { get; set; } // único

    [Required]
    public required string Titulo { get; set; }

    public required string Imagen { get; set; } // ruta o URL

    [Required]
    public TipoInmueble Tipo { get; set; }

    public required string Ciudad { get; set; }
    public required string Direccion { get; set; }

    [Range(0, int.MaxValue)]
    public int Dormitorios { get; set; }

    [Range(0, int.MaxValue)]
    public int Banos { get; set; }

    [Range(1, double.MaxValue)]
    public double MetrosCuadrados { get; set; } // > 0

    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; } // > 0

    public bool Activo { get; set; } = true;
    public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
}
