using SmartBook.Domain.Enums;

namespace SmartBook.Domain.Dtos.Requests;

public record ConsultarLibroRequest(
    string? Nombre,
    string? Nivel,
    TipoLibro? Tipo,
    string? Editorial,
    int? Edicion
);