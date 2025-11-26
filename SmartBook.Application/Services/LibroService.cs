using Org.BouncyCastle.Asn1.Ocsp;
using SmartBook.Application.Extensions;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Exceptions;
using SmartBook.Persistence.Repositories.Interface;

namespace SmartBook.Application.Services;
public class LibroService
{
    private readonly ILibroRepository _libroRepository;

    public LibroService(ILibroRepository libroRepository)
    {
        _libroRepository = libroRepository;
    }

    public LibroResponse? Crear(CrearLibroRequest request)
    {
        var librosConLaMismaCombinacion = _libroRepository.ValidarCreacionLibro(request.NombreLibro, request.NivelLibro, (int)request.TipoLibro,request.EdicionLibro);

        if (!librosConLaMismaCombinacion)
        {
            throw new BusinessRoleException(
                "Ya existe un libro con el mismo nombre, nivel, tipo y edición"
            );
        }

        var libro = new Libro
        {
            IdLibro = DateTime.Now.Ticks.ToString(),
            NombreLibro = request.NombreLibro.Sanitize().RemoveAccents(),
            NivelLibro = request.NivelLibro.Sanitize().RemoveAccents(),
            StockLibro = 0,
            TipoLibro = request.TipoLibro,
            EditorialLibro = request.EditorialLibro.Sanitize().RemoveAccents(),
            EdicionLibro = request.EdicionLibro
        };

        _libroRepository.Crear(libro);

        return new LibroResponse(libro.IdLibro, libro.NombreLibro, libro.NivelLibro);

    }

    public ConsultarLibroResponse? Consultar(string id)
    {
        return _libroRepository.Consultar(id);
    }
    public IEnumerable<ConsultarLibroResponse> Consultar(ConsultarLibroRequest request)
    {
        return _libroRepository.Consultar(request);
    }

    public bool Actualizar(string id, ActualizarLibroRequest request)
    {
        // Validar que el libro a actualizar existe
        var libroExiste = _libroRepository.Consultar(id);
        if (libroExiste is null)
        {
            return false; // El libro no existe
        }

        // Validar que la nueva combinación no colisione con otro libro
        // (excepto si los valores no cambiaron)
        var esValidoActualizar = _libroRepository.ValidarCreacionLibro(
            request.NombreLibro,
            request.NivelLibro,
            (int)request.TipoLibro,
            request.EdicionLibro
        );

        // Si la validación falla, verificamos si es porque estamos actualizando con los mismos valores
        if (!esValidoActualizar)
        {
            // Verificar si los valores son exactamente los mismos que ya tiene
            var libroActual = _libroRepository.Consultar(id);
            var esMismaCombinacion = libroActual!.NombreLibro == request.NombreLibro.Sanitize().RemoveAccents() &&
                                     libroActual.NivelLibro == request.NivelLibro.Sanitize().RemoveAccents() &&
                                     libroActual.TipoLibro == request.TipoLibro &&
                                     libroActual.EdicionLibro == request.EdicionLibro;

            if (!esMismaCombinacion)
            {
                throw new BusinessRoleException(
                    "Ya existe otro libro con el mismo nombre, nivel, tipo y edición"
                );
            }
        }

        // Aplicar sanitización antes de actualizar
        var requestSanitizado = new ActualizarLibroRequest(
            request.NombreLibro.Sanitize().RemoveAccents(),
            request.NivelLibro.Sanitize().RemoveAccents(),
            request.TipoLibro,
            request.EditorialLibro.Sanitize().RemoveAccents(),
            request.EdicionLibro
        );

        return _libroRepository.Actualizar(id, requestSanitizado);
    }

}