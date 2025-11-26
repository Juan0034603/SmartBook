using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record DetalleIngresoResponse(
    string IdDetalleIngreso,
    string IdLibro,
    string NombreLibro,
    string NivelLibro,
    int Unidades,
    decimal ValorCompra,
    decimal ValorVentaPublico,
    decimal SubtotalCompra
);
