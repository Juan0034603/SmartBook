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


public class IngresoEfcRepository : IIngresoRepository
{
    private readonly SmartBookDbContext _context;

    public IngresoEfcRepository(SmartBookDbContext context)
    {
        _context = context;
    }

    public void Crear(Ingreso ingreso)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            ingreso.Lote = GenerarCodigoLote();
            _context.Ingresos.Add(ingreso);

            foreach (var detalle in ingreso.Detalles)
            {
                var libro = _context.Libros.Find(detalle.IdLibro);
                if (libro != null)
                {
                    libro.StockLibro += detalle.Unidades;
                }
            }

            _context.SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public ConsultarIngresoResponse? Consultar(string id)
    {
        var ingreso = _context.Ingresos
            .Include(i => i.Detalles)
            .ThenInclude(d => d.Libro)
            .FirstOrDefault(i => i.IdIngreso == id);

        if (ingreso == null)
        {
            return null;
        }

        var detalles = ingreso.Detalles.Select(d => new DetalleIngresoResponse(
            d.IdDetalleIngreso,
            d.IdLibro,
            d.Libro.NombreLibro,
            d.Libro.NivelLibro,
            d.Unidades,
            d.ValorCompra,
            d.ValorVentaPublico,
            d.ValorCompra * d.Unidades
        )).ToList();

        var totalCompra = detalles.Sum(d => d.SubtotalCompra);
        var totalUnidades = detalles.Sum(d => d.Unidades);

        return new ConsultarIngresoResponse(
            ingreso.IdIngreso,
            ingreso.FechaIngreso,
            ingreso.Lote,
            detalles,
            totalCompra,
            totalUnidades
        );
    }

    private string GenerarCodigoLote()
    {
        var anioActual = DateTime.Now.Year;
        var ultimoLote = _context.Ingresos
            .Where(i => i.Lote.StartsWith($"{anioActual}-"))
            .OrderByDescending(i => i.Lote)
            .Select(i => i.Lote)
            .FirstOrDefault();

        if (ultimoLote == null)
        {
            return $"{anioActual}-1";
        }

        var partes = ultimoLote.Split('-');
        var consecutivo = int.Parse(partes[1]) + 1;
        return $"{anioActual}-{consecutivo}";
    }

    public IEnumerable<ConsultarIngresoResponse> Consultar(ConsultarIngresoRequest request)
    {
        var consulta = _context.Ingresos
            .Include(i => i.Detalles)
            .ThenInclude(d => d.Libro)
            .AsQueryable();

        // Filtro por rango de fechas
        if (request.Desde.HasValue)
        {
            var desdeDateTime = request.Desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(i => i.FechaIngreso >= desdeDateTime);
        }

        if (request.Hasta.HasValue)
        {
            var hastaDateTime = request.Hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(i => i.FechaIngreso <= hastaDateTime);
        }

        // Filtro por lote
        if (!string.IsNullOrWhiteSpace(request.Lote))
        {
            consulta = consulta.Where(i => i.Lote.Contains(request.Lote));
        }

        // Filtro por libro (si algún detalle contiene ese libro)
        if (!string.IsNullOrWhiteSpace(request.IdLibro))
        {
            consulta = consulta.Where(i => i.Detalles.Any(d => d.IdLibro == request.IdLibro));
        }

        // Proyectar a Response
        var resultados = consulta
            .OrderByDescending(i => i.FechaIngreso) // Los más recientes primero
            .Select(ingreso => new ConsultarIngresoResponse(
                ingreso.IdIngreso,
                ingreso.FechaIngreso,
                ingreso.Lote,
                ingreso.Detalles.Select(d => new DetalleIngresoResponse(
                    d.IdDetalleIngreso,
                    d.IdLibro,
                    d.Libro.NombreLibro,
                    d.Libro.NivelLibro,
                    d.Unidades,
                    d.ValorCompra,
                    d.ValorVentaPublico,
                    d.ValorCompra * d.Unidades
                )).ToList(),
                ingreso.Detalles.Sum(d => d.ValorCompra * d.Unidades), // TotalCompra
                ingreso.Detalles.Sum(d => d.Unidades) // TotalUnidades
            ))
            .ToList();

        return resultados;
    }











    public int ObtenerStockDisponible(string libroId)
    {
        /*
        SELECT SUM(Unidades) 
        FROM DetalleIngresos 
        WHERE IdLibro = @libroId AND Unidades > 0
        */

        return _context.DetalleIngresos
            .Where(d => d.IdLibro == libroId && d.Unidades > 0)
            .Sum(d => (int?)d.Unidades) ?? 0;
    }

    public List<DetalleIngresos> ObtenerLotesPorLibro(string libroId)
    {
        /*
        SELECT di.*, i.Lote, i.FechaIngreso
        FROM DetalleIngresos di
        INNER JOIN Ingresos i ON di.IdIngreso = i.IdIngreso
        WHERE di.IdLibro = @libroId AND di.Unidades > 0
        ORDER BY i.FechaIngreso ASC
        */

        return _context.DetalleIngresos
            .Include(d => d.Ingreso) // ← Incluye Ingreso para obtener Lote y FechaIngreso
            .Where(d => d.IdLibro == libroId && d.Unidades > 0)
            .OrderBy(d => d.Ingreso.FechaIngreso) // ← FIFO: más viejo primero
            .ToList();
    }

    public bool DescontarUnidades(string idDetalleIngreso, int cantidad)
    {
        /*
        UPDATE DetalleIngresos 
        SET Unidades = Unidades - @cantidad 
        WHERE IdDetalleIngreso = @idDetalleIngreso
        */

        var detalle = _context.DetalleIngresos
            .FirstOrDefault(d => d.IdDetalleIngreso == idDetalleIngreso);

        if (detalle is null)
        {
            return false;
        }

        detalle.Unidades -= cantidad;

        // Validar que no quede negativo
        if (detalle.Unidades < 0)
        {
            detalle.Unidades = 0;
        }

        return _context.SaveChanges() > 0;
    }

}
    
