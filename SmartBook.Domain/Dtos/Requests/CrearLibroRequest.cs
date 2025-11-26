using SmartBook.Domain.Enums;

namespace SmartBook.Domain.Dtos.Requests;

public record CrearLibroRequest(
    string NombreLibro,
    string NivelLibro,
    TipoLibro TipoLibro,
    string EditorialLibro,
    int EdicionLibro
);
