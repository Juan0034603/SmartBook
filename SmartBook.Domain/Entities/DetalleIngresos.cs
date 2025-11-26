using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Entities;

[Table("DetalleIngresos")]
public class DetalleIngresos
{
    [Key]
    [Column("IdDetalleIngreso")]
    public string IdDetalleIngreso { get; set; }

    [Column("IdIngreso")]
    public string IdIngreso { get; set; }

    [Column("IdLibro")]
    public string IdLibro { get; set; }

    [Column("Unidades")]
    public int Unidades { get; set; }

    [Column("ValorCompra")]
    public decimal ValorCompra { get; set; }

    [Column("ValorVentaPublico")]
    public decimal ValorVentaPublico { get; set; }

    public Ingreso Ingreso { get; set; }
    public Libro Libro { get; set; }
}
