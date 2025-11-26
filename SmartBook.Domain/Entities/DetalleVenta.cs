using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Entities;

[Table("DetalleVentas")]
public class DetalleVenta
{
    [Key]
    [Column("Id")]
    public string Id { get; init; }

    [Column("VentaId")]
    public string VentaId { get; set; }

    [Column("LibroId")]
    public string LibroId { get; set; }

    [Column("Lote")]
    public string Lote { get; set; }

    [Column("Cantidad")]
    public int Cantidad { get; set; }

    [Column("PrecioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [Column("FechaCreacion")]
    public DateTime FechaCreacion { get; init; }
}