using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Requests;

public record LoginRequest
(
    string CorreoUsuario,
    string ContraseniaUsuario
);