using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Entities;

[Table("ventas")]
public class VentaLibro
{
    [Key]
    [Column("Id")]
    public string Id { get; init; }

    [Column("NumeroReciboPago")]
    public string NumeroReciboPago { get; set; }

    [Column("Fecha")]
    public DateTime Fecha { get; init; }

    [Column("ClienteIdentificacion")]
    public string ClienteIdentificacion { get; set; }

    [Column("UsuarioId")]
    public string UsuarioId { get; set; }

    [Column("Observaciones")]
    public string? Observaciones { get; set; }

    [Column("FechaCreacion")]
    public DateTime FechaCreacion { get; init; }

    [Column("FechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    // ✅ Relación con DetalleVenta
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}

