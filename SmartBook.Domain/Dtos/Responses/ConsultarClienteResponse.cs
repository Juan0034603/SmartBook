using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record ConsultarClienteResponse(
    string Id,
    string Identificacion,
    string Nombres,
    string Email,
    string Celular,
    DateOnly FechaNacimiento,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion
);