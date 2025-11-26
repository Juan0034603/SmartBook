namespace SmartBook.Domain.Dtos.Requests;

public record ConsultarClienteRequest
(
    string? Nombres,
    string? Email,
    string? Celular,
    string? Identificacion
);
