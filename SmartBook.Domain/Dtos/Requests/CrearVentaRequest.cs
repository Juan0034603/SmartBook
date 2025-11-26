using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Requests;

public record CrearVentaRequest
(
    string NumeroReciboPago,

    string ClienteIdentificacion,

    string UsuarioId,

    string? Observaciones,


    List<CrearVentaDetalleRequest> Detalles


    );