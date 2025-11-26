namespace SmartBook.Domain.Dtos.Requests;

public record ActualizarClienteRequest(
    string Nombres,
    string Email,
    string Celular,
    DateOnly FechaNacimiento
);


