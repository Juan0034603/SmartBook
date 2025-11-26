using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;

namespace SmartBook.Persistence.Repositories.Interface;

public interface ILibroRepository
{
    bool ValidarCreacionLibro(string nombreLibro, string nivelLibro, int tipoLibro, int edicionLibro);
    void Crear(Libro libro);
    ConsultarLibroResponse? Consultar(string id);
    IEnumerable<ConsultarLibroResponse> Consultar(ConsultarLibroRequest request);
    bool Actualizar(string id, ActualizarLibroRequest request);
    bool ActualizarStockVenta(string id, int nuevaCantidad);

}
