using Microsoft.EntityFrameworkCore;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Persistence.DbContexts;
using SmartBook.Persistence.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories;
public class VentaEfcRepository(SmartBookDbContext context) : IVentaRepository
{
    private readonly SmartBookDbContext _context = context;

    public void Crear(VentaLibro venta)
    {
        /*
        INSERT INTO ventas (Id, NumeroReciboPago, Fecha, ClienteId, UsuarioId, Observaciones, FechaCreacion)
        VALUES (@Id, @NumeroReciboPago, @Fecha, @ClienteId, @UsuarioId, @Observaciones, @FechaCreacion);
        
        INSERT INTO detalle_ventas (Id, VentaId, LibroId, Lote, Cantidad, PrecioUnitario, FechaCreacion)
        VALUES (@Id, @VentaId, @LibroId, @Lote, @Cantidad, @PrecioUnitario, @FechaCreacion);
        */

        _context.VentasLibros.Add(venta);
        _context.SaveChanges();
    }

    public VentaResponse? Consultar(string id)
    {
        /*
        SELECT v.*, c.Nombres, c.Email, d.*, l.NombreLibro, l.NivelLibro
        FROM ventas v
        INNER JOIN clientes c ON v.ClienteId = c.Id
        INNER JOIN detalle_ventas d ON v.Id = d.VentaId
        INNER JOIN libros l ON d.LibroId = l.idLibro
        WHERE v.Id = @id
        */

        var venta = _context.VentasLibros
            .Include(v => v.Detalles)
            .FirstOrDefault(v => v.Id == id);

        if (venta is null)
        {
            return null;
        }

        var cliente = _context.Clientes
            .FirstOrDefault(c => c.Id == venta.ClienteIdentificacion);

        var detalles = venta.Detalles.Select(d =>
        {
            var libro = _context.Libros.FirstOrDefault(l => l.IdLibro == d.LibroId);
            return new VentaDetalleResponse(
                libro?.NombreLibro ?? "Desconocido",
                libro?.NivelLibro ?? "",
                d.Lote,
                d.Cantidad,
                d.PrecioUnitario,
                d.Cantidad * d.PrecioUnitario
            );
        }).ToList();

        var montoTotal = detalles.Sum(d => d.Subtotal);

        return new VentaResponse(
            venta.NumeroReciboPago,
            venta.Fecha,
            cliente?.Nombres ?? "Desconocido",
            cliente?.Email ?? "",
            venta.Observaciones,
            detalles,
            montoTotal
        );
    }

    public IEnumerable<VentaResponse> Consultar(ConsultarVentaRequest request)
    {
        /*
        SELECT v.*, c.Nombres, c.Email, d.*, l.NombreLibro, l.NivelLibro
        FROM ventas v
        INNER JOIN clientes c ON v.ClienteId = c.Id
        INNER JOIN detalle_ventas d ON v.Id = d.VentaId
        INNER JOIN libros l ON d.LibroId = l.idLibro
        WHERE 
            (v.Fecha >= @Desde OR @Desde IS NULL) AND
            (v.Fecha <= @Hasta OR @Hasta IS NULL) AND
            (v.ClienteId = @ClienteId OR @ClienteId IS NULL) AND
            (d.LibroId = @LibroId OR @LibroId IS NULL)
        */

        var consulta = _context.VentasLibros
            .Include(v => v.Detalles)
            .AsQueryable();

        // Filtrar por rango de fechas
        if (request.Desde.HasValue)
        {
            consulta = consulta.Where(v => v.Fecha >= request.Desde.Value);
        }

        if (request.Hasta.HasValue)
        {
            consulta = consulta.Where(v => v.Fecha <= request.Hasta.Value);
        }

        // Filtrar por ClienteId
        if (!string.IsNullOrWhiteSpace(request.ClienteId))
        {
            consulta = consulta.Where(v => v.ClienteIdentificacion == request.ClienteId);
        }

        // Filtrar por LibroId (en los detalles)
        if (!string.IsNullOrWhiteSpace(request.LibroId))
        {
            consulta = consulta.Where(v => v.Detalles.Any(d => d.LibroId == request.LibroId));
        }

        var ventas = consulta.ToList();

        // Mapear a VentaResponse
        var resultados = ventas.Select(venta =>
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == venta.ClienteIdentificacion);

            var detalles = venta.Detalles.Select(d =>
            {
                var libro = _context.Libros.FirstOrDefault(l => l.IdLibro == d.LibroId);
                return new VentaDetalleResponse(
                    libro?.NombreLibro ?? "Desconocido",
                    libro?.NivelLibro ?? "",
                    d.Lote,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Cantidad * d.PrecioUnitario
                );
            }).ToList();

            var montoTotal = detalles.Sum(d => d.Subtotal);

            return new VentaResponse(
                venta.NumeroReciboPago,
                venta.Fecha,
                cliente?.Nombres ?? "Desconocido",
                cliente?.Email ?? "",
                venta.Observaciones,
                detalles,
                montoTotal
            );
        });

        return resultados;
    }
}
