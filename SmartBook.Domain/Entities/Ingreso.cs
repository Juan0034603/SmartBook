using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Entities;

[Table("Ingresos")]
public class Ingreso
{
    [Key]
    [Column("IdIngreso")]
    public string IdIngreso { get; set; }

    [Column("FechaIngreso")]
    public DateTime FechaIngreso { get; init; }

    [Column("Lote")]
    public string Lote { get; set; }

    public ICollection<DetalleIngresos> Detalles { get; set; } = new List<DetalleIngresos>();

}
