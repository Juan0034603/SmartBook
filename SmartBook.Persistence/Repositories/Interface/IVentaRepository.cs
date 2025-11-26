using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories.Interface;

public interface IVentaRepository
{
    void Crear(VentaLibro venta);
    VentaResponse? Consultar(string id);
    IEnumerable<VentaResponse> Consultar(ConsultarVentaRequest request);
}
