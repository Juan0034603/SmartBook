using SmartBook.Domain.Enums;

namespace SmartBook.Domain.Dtos.Requests;

public record CrearUsuarioRequest
(
    string IdentificacionUsuario,
    string NombresUsuario,
    string ContraseniaUSuario,
    string CorreoUsuario,
    RolUsuario RolUsuario
 );