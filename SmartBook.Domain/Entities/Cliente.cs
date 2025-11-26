using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBook.Domain.Entities;

[Table("clientes")]
public class Cliente
{
    [Key]
    [Column("Id")]
    public string Id { get; init; }

    [Column("Identificacion")]
    public string Identificacion { get; set; }

    [Column("Nombres")]
    public string Nombres { get; set; }

    [Column("Email")]
    public string Email { get; set; }

    [Column("Celular")]
    public string Celular { get; set; }

    [Column("FechaNacimiento")]
    public DateOnly FechaNacimiento { get; set; }

    [Column("FechaCreacion")]
    public DateTime FechaCreacion { get; init; }

    [Column("FechaActualizacion")]
    public DateTime? FechaActualizacion { get; set; }
}