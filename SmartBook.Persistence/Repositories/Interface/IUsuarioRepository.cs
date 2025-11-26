using Org.BouncyCastle.Asn1.Ocsp;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories.Interface
{
    public interface IUsuarioRepository
    {
        Task Crear(Usuario usuario);

        IEnumerable<ConsultarUsuarioResponse> Consultar(ConsultarUsuarioRequest consultarUsuario);

        bool ExistePorIdentificacion(string IdentificacionUsuario);
        public bool ExistePorCorreo(string correo);

        Task<bool> Actualizar(string IdentificacionUsuario, ActualizarUsuarioRequest request);
        // ✨ NUEVOS MÉTODOS
        Task<Usuario?> ObtenerPorToken(string token);
        Task ActualizarEstadoCorreo(Usuario usuario);


        Task<Usuario?> ObtenerPorCorreo(string correo);

        Task<Usuario?> ObtenerPorIdentificacion(string IdentificacionUsuario);
        Task ActualizarContraseña(Usuario usuario);

    }
}
