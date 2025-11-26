using SmartBook.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBook.Domain.Entities;


[Table("libros")]
public class Libro
{
    [Key]
    [Column("idLibro")]
    public string IdLibro { get; init; }

    [Column("nombreLibro")]
    public string NombreLibro { get; set; }

    [Column("nivelLibro")]
    public string NivelLibro { get; set; }

    [Column("stockLibro")]
    public int StockLibro { get; set; }

    [Column("tipoLibro")]
    public TipoLibro TipoLibro { get; set; }

    [Column("editorialLibro")]
    public string EditorialLibro { get; set; }

    [Column("edicionLibro")]
    public int EdicionLibro { get; set; }
}


