using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories.Interface;

public interface IIngresoRepository
{
    void Crear(Ingreso ingreso);
    ConsultarIngresoResponse? Consultar(string id);
    IEnumerable<ConsultarIngresoResponse> Consultar(ConsultarIngresoRequest request);

    int ObtenerStockDisponible(string libroId);

    List<DetalleIngresos> ObtenerLotesPorLibro(string libroId);

    bool DescontarUnidades(string idDetalleIngreso, int cantidad);

}



