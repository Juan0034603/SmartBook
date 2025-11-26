namespace SmartBook.Domain.Dtos.Requests;

public record CreateClienteRequest(
    string Identificacion,
    string Nombres,
    string Email,
    string Celular,
    DateOnly FechaNacimiento

);
