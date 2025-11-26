using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record IngresoResponse(
    string Id,
    string Lote,
    DateTime Fecha
);