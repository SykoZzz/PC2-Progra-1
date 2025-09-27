using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalInmobiliario.Models
{
public class Reserva
{
    public int Id { get; set; }
    public int InmuebleId { get; set; }
    public string? UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }

    public Inmueble? Inmueble { get; set; }
}

}