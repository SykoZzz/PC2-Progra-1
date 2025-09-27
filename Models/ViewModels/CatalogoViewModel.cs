using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalInmobiliario.Models.ViewModels
{
public class CatalogoViewModel
{
    public IEnumerable<Inmueble> Inmuebles { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }

    // filtros
    public string Ciudad { get; set; }
    public TipoInmueble? Tipo { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public int? Dormitorios { get; set; }
}
}