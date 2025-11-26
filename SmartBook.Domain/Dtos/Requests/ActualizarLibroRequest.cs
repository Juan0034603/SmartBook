using SmartBook.Domain.Enums;

namespace SmartBook.Domain.Dtos.Requests;

public record ActualizarLibroRequest(
    string NombreLibro,
    string NivelLibro,
    TipoLibro TipoLibro,
    string EditorialLibro,
    int EdicionLibro
);