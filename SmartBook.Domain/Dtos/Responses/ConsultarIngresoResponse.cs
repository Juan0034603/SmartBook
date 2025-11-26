namespace SmartBook.Domain.Dtos.Responses;

public record ConsultarIngresoResponse(
    string IdIngreso,
    DateTime FechaIngreso,
    string Lote,
    List<DetalleIngresoResponse> Detalles,
    decimal TotalCompra,
    int TotalUnidades
);