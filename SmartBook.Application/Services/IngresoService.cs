using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Exceptions;
using SmartBook.Persistence.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Services;

public class IngresoService
{
    private readonly IIngresoRepository _ingresoRepository;
    private readonly ILibroRepository _libroRepository;

    public IngresoService(IIngresoRepository ingresoRepository, ILibroRepository libroRepository)
    {
        _ingresoRepository = ingresoRepository;
        _libroRepository = libroRepository;
    }

    public IngresoResponse? Crear(CrearIngresoRequest request)
    {
        if (request.Detalles == null || !request.Detalles.Any())
        {
            throw new BusinessRoleException("Debe incluir al menos un libro en el ingreso");
        }

        foreach (var detalle in request.Detalles)
        {
            var libroExiste = _libroRepository.Consultar(detalle.IdLibro);

            if (libroExiste is null)
            {
                throw new BusinessRoleException($"El libro con ID {detalle.IdLibro} no existe");
            }
            if (detalle.Unidades <= 0)
            {
                throw new BusinessRoleException("Las unidades deben ser mayores a 0");
            }
            if (detalle.ValorCompra <= 0)
            {
                throw new BusinessRoleException("El valor de compra debe ser mayor a 0");
            }
            if (detalle.ValorVentaPublico <= 0)
            {
                throw new BusinessRoleException("El valor de venta al público debe ser mayor a 0");
            }
        }

        var ingreso = new Ingreso
        {
            IdIngreso = DateTime.Now.Ticks.ToString(),
            FechaIngreso = DateTime.Now
        };

        foreach (var detalleRequest in request.Detalles)
        {
            var detalle = new DetalleIngresos
            {
                IdDetalleIngreso = DateTime.Now.Ticks.ToString() + detalleRequest.IdLibro,
                IdIngreso = ingreso.IdIngreso,
                IdLibro = detalleRequest.IdLibro,
                Unidades = detalleRequest.Unidades,
                ValorCompra = detalleRequest.ValorCompra,
                ValorVentaPublico = detalleRequest.ValorVentaPublico
            };
            ingreso.Detalles.Add(detalle);
        }

        _ingresoRepository.Crear(ingreso);
        return new IngresoResponse(ingreso.IdIngreso, ingreso.Lote, ingreso.FechaIngreso);
    }

    public ConsultarIngresoResponse? Consultar(string id)
    {
        return _ingresoRepository.Consultar(id);
    }

    public IEnumerable<ConsultarIngresoResponse> Consultar(ConsultarIngresoRequest request)
    {
        return _ingresoRepository.Consultar(request);
    }
}