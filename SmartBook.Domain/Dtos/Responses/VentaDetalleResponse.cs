using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Dtos.Responses;

public record VentaDetalleResponse
(
    string LibroNombre,
    string LibroNivel,
    string Lote,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);