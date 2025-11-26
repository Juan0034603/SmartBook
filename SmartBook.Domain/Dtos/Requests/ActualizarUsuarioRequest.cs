using SmartBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Requests;

public record ActualizarUsuarioRequest
( 
    RolUsuario? RolUsuario,
    EstadoUsuario? EstadoUsuario
);

   


