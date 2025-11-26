using SmartBook.Domain.Enums;

namespace SmartBook.Domain.Dtos.Responses;

public record ConsultarLibroResponse(
    string IdLibro,
    string NombreLibro,
    string NivelLibro,
    int StockLibro,
    TipoLibro TipoLibro,
    string EditorialLibro,
    int EdicionLibro
);
