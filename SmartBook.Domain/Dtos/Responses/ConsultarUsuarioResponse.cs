using SmartBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record ConsultarUsuarioResponse
(


string IdentificacionUsuario,
string NombresUsuario,
string CorreoUsuario,
RolUsuario RolUsuario,
EstadoUsuario EstadoUsuario,
DateTime FechaCreacion
 );