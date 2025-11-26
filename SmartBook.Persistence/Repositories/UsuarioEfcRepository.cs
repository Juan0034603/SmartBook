using Microsoft.EntityFrameworkCore;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Enums;
using SmartBook.Persistence.DbContexts;
using SmartBook.Persistence.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartBook.Persistence.Repositories;

public class UsuarioEfcRepository(SmartBookDbContext context) : IUsuarioRepository
{
    private readonly SmartBookDbContext _context = context;

    public async Task Crear(Usuario usuario)
    {

        /*
         
        INSERT INTO cursos VALUES(@id,@nombre,@fechaInicio,@fechaFinalizacion,@fechaCreacion,@docenteId,@estado,@horasSemanales)
         */
        await _context.Usuarios.AddAsync(usuario);  // ← Usa AddAsync
        await _context.SaveChangesAsync();          // ← Usa SaveChangesAsync
    }



    public IEnumerable<ConsultarUsuarioResponse> Consultar(ConsultarUsuarioRequest consultarUsuario)
    {
        var consulta = _context.Usuarios.AsQueryable();

        // Filtramos según los parámetros de la solicitud
        if (consultarUsuario.NombresUsuario is not null)
        {
            consulta = consulta.Where(c => c.NombresUsuario == consultarUsuario.NombresUsuario);
        }

        if (consultarUsuario.RolUsuario is not null)
        {
            consulta = consulta.Where(c => c.RolUsuario == consultarUsuario.RolUsuario);
        }


        // Realizamos el JOIN con los docentes
        var resultados = consulta
     .Select(usuario => new ConsultarUsuarioResponse(
         usuario.IdentificacionUsuario,
         usuario.NombresUsuario,
         usuario.CorreoUsuario,
         usuario.RolUsuario,
         usuario.EstadoUsuario,
         usuario.FechaCreacion
     ))
     .AsEnumerable();


        return resultados;
    }



    public async Task<bool> Actualizar(string IdentificacionUsuario, ActualizarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdentificacionUsuario == IdentificacionUsuario);

        if (usuario == null)
            return false;

        // Actualizar solo los campos enviados
        if (request.EstadoUsuario.HasValue)
            usuario.EstadoUsuario = request.EstadoUsuario.Value;

        if (request.RolUsuario.HasValue)
            usuario.RolUsuario = request.RolUsuario.Value;

        await _context.SaveChangesAsync();  // ✅ async
        return true;
    }







    public bool ExistePorIdentificacion(string IdentificacionUsuario)
    {
        return _context.Usuarios
            .Any(u => u.IdentificacionUsuario == IdentificacionUsuario);
    }

    public bool ExistePorCorreo(string correo)
    {
        return _context.Usuarios.Any(u => u.CorreoUsuario == correo);
    }

    public async Task<Usuario?> ObtenerPorToken(string token)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.TokenConfirmacion == token);
    }

    public async Task ActualizarEstadoCorreo(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }


    public async Task<Usuario?> ObtenerPorCorreo(string correo)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.CorreoUsuario == correo);
    }



    public async Task<Usuario?> ObtenerPorIdentificacion(string id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    public async Task ActualizarContraseña(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
