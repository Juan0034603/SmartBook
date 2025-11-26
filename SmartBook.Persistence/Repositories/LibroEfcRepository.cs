using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Enums;
using SmartBook.Persistence.DbContexts;
using SmartBook.Persistence.Repositories.Interface;

namespace SmartBook.Persistence.Repositories;


public class LibroEfcRepository(SmartBookDbContext context) : ILibroRepository
{
    private readonly SmartBookDbContext _context = context;

    public void Crear(Libro libro)
    {

        _context.Libros.Add(libro);
        _context.SaveChanges();
    }

    public bool ValidarCreacionLibro(string nombreLibro, string nivelLibro, int tipoLibro, int edicionLibro)
    {

        return !_context.Libros.Any(l => l.NombreLibro == nombreLibro && l.NivelLibro == nivelLibro &&  l.TipoLibro == (TipoLibro)tipoLibro &&  l.EdicionLibro == edicionLibro
        
        );
    }

    public ConsultarLibroResponse? Consultar(string id)
    {

        return _context.Libros
            .Where(libro => libro.IdLibro == id)
            .Select(libro => new ConsultarLibroResponse(
                libro.IdLibro,
                libro.NombreLibro,
                libro.NivelLibro,
                libro.StockLibro,
                libro.TipoLibro,
                libro.EditorialLibro,
                libro.EdicionLibro
            ))
            .FirstOrDefault(); 
    }

    public IEnumerable<ConsultarLibroResponse> Consultar(ConsultarLibroRequest request)
    {
        var consulta = _context.Libros.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
        {
            consulta = consulta.Where(l => l.NombreLibro.Contains(request.Nombre));
        }

        if (!string.IsNullOrWhiteSpace(request.Nivel))
        {
            consulta = consulta.Where(l => l.NivelLibro.Contains(request.Nivel));
        }

        if (request.Tipo is not null)
        {
            consulta = consulta.Where(l => l.TipoLibro == request.Tipo);
        }

        if (!string.IsNullOrWhiteSpace(request.Editorial))
        {
            consulta = consulta.Where(l => l.EditorialLibro.Contains(request.Editorial));
        }

        if (request.Edicion is not null)
        {
            consulta = consulta.Where(l => l.EdicionLibro == request.Edicion);
        }

        var resultados = consulta
            .Select(l => new ConsultarLibroResponse(
                l.IdLibro,
                l.NombreLibro,
                l.NivelLibro,
                l.StockLibro,
                l.TipoLibro,
                l.EditorialLibro,
                l.EdicionLibro
            ))
            .ToList(); 

        return resultados;
    }

    public bool Actualizar(string id, ActualizarLibroRequest request)
    {

        var libro = _context.Libros.Find(id);

        if (libro is null)
        {
            return false; // Libro no encontrado
        }

        // Actualizamos los campos
        libro.NombreLibro = request.NombreLibro;
        libro.NivelLibro = request.NivelLibro;
        libro.TipoLibro = request.TipoLibro;
        libro.EditorialLibro = request.EditorialLibro;
        libro.EdicionLibro = request.EdicionLibro;

        return _context.SaveChanges() > 0;
    }
    public bool ActualizarStockVenta(string id, int nuevaCantidad)
    {
        var libro = _context.Libros.FirstOrDefault(l => l.IdLibro == id);

        if (libro is null)
        {
            return false;
        }

        libro.StockLibro = nuevaCantidad;
        return _context.SaveChanges() > 0;
    }

}