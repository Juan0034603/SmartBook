using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Persistence.DbContexts;
using SmartBook.Persistence.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories;

public class ClienteEfcRepository(SmartBookDbContext context) : IClienteRepository
{
    private readonly SmartBookDbContext _context = context;

    public void Crear(Cliente cliente)
    {

        _context.Clientes.Add(cliente);
        _context.SaveChanges();

    }

    public bool ValidarCreacionCliente(string identificacion, string email, string celular)
    {
        /*
        SELECT COUNT(id) 
        FROM clientes 
        WHERE identificacion = @identificacion OR email = @email OR celular = @celular
        */
        return !_context.Clientes.Any(c =>
            c.Identificacion == identificacion ||
            c.Email == email ||
            c.Celular == celular
        );
    }

    public ConsultarClienteResponse? Consultar(string identificacion)
    {
        /*
        SELECT Id, Identificacion, Nombres, Email, Celular, 
               FechaNacimiento, FechaCreacion, FechaActualizacion
        FROM clientes 
        WHERE Identificacion = @identificacion
        */

        return _context.Clientes
            .Where(c => c.Identificacion == identificacion)
            .Select(c => new ConsultarClienteResponse(
                c.Id,
                c.Identificacion,
                c.Nombres,
                c.Email,
                c.Celular,
                c.FechaNacimiento,
                c.FechaCreacion,
                c.FechaActualizacion
            ))
            .FirstOrDefault();
    }

    public IEnumerable<ConsultarClienteResponse> Consultar(ConsultarClienteRequest request)
    {
        /*
        SELECT Id, Identificacion, Nombres, Email, Celular, 
               FechaNacimiento, FechaCreacion, FechaActualizacion
        FROM clientes 
        WHERE 
            (Nombres LIKE '%@nombres%' OR @nombres IS NULL) AND
            (Email LIKE '%@email%' OR @email IS NULL) AND
            (Celular = @celular OR @celular IS NULL) AND
            (Identificacion = @identificacion OR @identificacion IS NULL)
        */

        var consulta = _context.Clientes.AsQueryable();

        // Filtrar por Nombres (búsqueda parcial)
        if (!string.IsNullOrWhiteSpace(request.Nombres))
        {
            consulta = consulta.Where(c => c.Nombres.Contains(request.Nombres));
        }

        // Filtrar por Email (búsqueda parcial)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            consulta = consulta.Where(c => c.Email.Contains(request.Email));
        }

        // Filtrar por Celular (búsqueda exacta)
        if (!string.IsNullOrWhiteSpace(request.Celular))
        {
            consulta = consulta.Where(c => c.Celular == request.Celular);
        }

        // Filtrar por Identificación (búsqueda exacta)
        if (!string.IsNullOrWhiteSpace(request.Identificacion))
        {
            consulta = consulta.Where(c => c.Identificacion == request.Identificacion);
        }

        // Proyectar a DTO
        var resultados = consulta
            .Select(c => new ConsultarClienteResponse(
                c.Id,
                c.Identificacion,
                c.Nombres,
                c.Email,
                c.Celular,
                c.FechaNacimiento,
                c.FechaCreacion,
                c.FechaActualizacion
            ))
            .AsEnumerable();

        return resultados;
    }

    public bool Actualizar(string identificacion, ActualizarClienteRequest request)
    {
        // Buscar el cliente por identificación
        var cliente = _context.Clientes
            .FirstOrDefault(c => c.Identificacion == identificacion);

        // Si no existe, retornar false
        if (cliente is null)
        {
            return false;
        }

        // Actualizar los campos
        cliente.Nombres = request.Nombres;
        cliente.Email = request.Email;
        cliente.Celular = request.Celular;
        cliente.FechaNacimiento = request.FechaNacimiento;
        cliente.FechaActualizacion = DateTime.Now; // ✅ Actualizar fecha

        // Guardar cambios
        return _context.SaveChanges() > 0;
    }
}