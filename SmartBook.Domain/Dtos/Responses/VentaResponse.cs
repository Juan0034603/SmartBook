using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record VentaResponse
(
    string NumeroReciboPago,
    DateTime Fecha,
    string ClienteNombre,
    string ClienteEmail,
    string? Observaciones,
    List<VentaDetalleResponse> Detalles,
    decimal MontoTotal
);