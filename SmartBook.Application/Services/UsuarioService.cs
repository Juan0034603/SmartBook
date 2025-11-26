using Microsoft.AspNetCore.Http;  // ← AGREGAR ESTE
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SmartBook.Application.Extensions;
using SmartBook.Application.Helpers;
using SmartBook.Application.Interface;
using SmartBook.Application.Options;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Enums;
using SmartBook.Domain.Exceptions;
using SmartBook.Persistence.Repositories.Interface;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SmartBook.Application.Services;

public class UsuarioService
{

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    private readonly CorreosInstitucionalesOption _correosOption;
    private readonly IEmailService _emailService;
    private readonly JwtOption _jwtOption;  // ← NUEVO
    private readonly IHttpContextAccessor _httpContextAccessor;




    public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration,
        IOptions<CorreosInstitucionalesOption> correosOption, IEmailService emailService, IOptions<JwtOption> jwtOption,
          IHttpContextAccessor httpContextAccessor)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
        _correosOption = correosOption.Value;
        _emailService = emailService;
        _jwtOption = jwtOption.Value;  // ← NUEVO
        _httpContextAccessor = httpContextAccessor;  // ← Agregar




    }


    //Task representa una operación asíncrona (que toma tiempo y no bloquea el programa).
    // 3. Enviar correo (toma tiempo - 2-3 segundos)
    // ✅ Libera el hilo mientras espera

    // async (Código Asíncrono)
    public async Task<UsuarioResponse?> Crear(CrearUsuarioRequest request)
    {


        // ========== VALIDACIÓN DE PERMISOS ==========

        // Obtener el usuario actual desde el token (si existe)
        var usuarioActual = _httpContextAccessor.HttpContext?.User;
        var rolActual = usuarioActual?.FindFirst(ClaimTypes.Role)?.Value;
        var estaAutenticado = usuarioActual?.Identity?.IsAuthenticated ?? false;

        // Si intenta crear un Admin
          if (request.RolUsuario == RolUsuario.Admin)
          {
              // Y no está autenticado → Bloquear
              if (!estaAutenticado)
              {
                  throw new UnauthorizedException(
                      "Debes iniciar sesión para crear usuarios Admin");
              }

              // Y está autenticado pero NO es Admin → Bloquear
              if (rolActual != "Admin")
              {
                  throw new ForbiddenExeption(
                      "Solo los administradores pueden crear usuarios con rol Admin");
              }
          }
        

        // vitar que se pueda mandar el correo dos veces
        //Probar si deja madnar nombre basio


        // 1. Validar identificación (formato)
        if (!Regex.IsMatch(request.IdentificacionUsuario, @"^[a-zA-Z0-9]+$"))
        {
            throw new BadRequestException("La identificación solo puede contener letras y números");
        }


        // ✔ Validar que los nombres no estén vacíos
        if (string.IsNullOrWhiteSpace(request.NombresUsuario))
        {
            throw new BadRequestException("Los nombres no pueden estar vacíos");
        }

        //Validar si existe la identificacion en la base de datos
        var identificacionExiste = _usuarioRepository.ExistePorIdentificacion(request.IdentificacionUsuario);
        if (identificacionExiste)
        {
            throw new BadRequestException("La identificación ya está registrada");

        }

        // ✔ Validar que el correo no exista
        var correoExiste =  _usuarioRepository.ExistePorCorreo(request.CorreoUsuario);
        if (correoExiste)
        {
            throw new BadRequestException("El correo ya está registrado");
        }


        //Falta Existe por correo


        // Validar formato de correo
        if (!CorreoHelper.EsFormatoValido(request.CorreoUsuario))
        {
            throw new BadRequestException("El formato del correo no es válido");
        }

        // Validar correo institucional
        if (!CorreoHelper.EsCorreoInstitucional(request.CorreoUsuario, _correosOption.Dominios))
        {
            throw new BadRequestException("Solo se aceptan correos institucionales");
        }
        // 5. Validar longitud de contraseña
        if (request.ContraseniaUSuario.Length < 8)
        {
            throw new BadRequestException("La contraseña debe tener mínimo 8 caracteres");
        }

        // ========== PROCESAMIENTO ==========

        // 6. Encriptar contraseña
        var contrasenaEncriptada = PasswordHelper.Encriptar(request.ContraseniaUSuario);


        var token = Guid.NewGuid().ToString();
        var expiracion = DateTime.Now.AddHours(1); // ← 1 hora


        var usuario = new Usuario
        {
            IdUsuario = DateTime.Now.Ticks.ToString(),
            IdentificacionUsuario = request.IdentificacionUsuario,
            NombresUsuario = request.NombresUsuario.Sanitize().RemoveAccents(),
            ContraseniaUSuario = contrasenaEncriptada,
            CorreoUsuario = request.CorreoUsuario,
            RolUsuario = request.RolUsuario,
            EstadoUsuario = EstadoUsuario.Activo,
            FechaCreacion = DateTime.Now,
            EmailConfirmado = false, // ← Nuevo campo
            TokenConfirmacion = token, // ← Sin token por ahora
            TokenExpiracion = expiracion    // ← Sin expiración por ahora
        };




        await _usuarioRepository.Crear(usuario);




        // ========== ✨ NUEVO: CONSTRUIR LINK DE CONFIRMACIÓN ==========
        // ⚠️ Cambia el puerto según tu launchSettings.json
        var linkConfirmacion = $"https://localhost:5048/api/usuarios/confirmar-email?token={token}";


        //En EL cuerpo del correo debe ir el logotipo de cecar
        // ========== ✨ NUEVO: CONSTRUIR CUERPO DEL CORREO ==========
        var cuerpoCorreo = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
.logo {{ text-align: center; padding: 20px 0; }}
.logo img {{ max-width: 180px; height: auto; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ 
                        display: inline-block;
                        background-color: #4CAF50;
                        color: white;
                        padding: 12px 30px;
                        text-decoration: none;
                        border-radius: 5px;
                        margin: 20px 0;
                    }}
                    .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
<div class='logo'>
    <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRFs1EpEDt7-pfZ8rTh2qnwEUvvC45K8RhuZw&s' alt='CECAR Logo'>
</div>
                    <div class='header'>
                        <h1>Bienvenido a SmartBook</h1>
                    </div>
                    <div class='content'>
                        <h2>Hola {usuario.NombresUsuario},</h2>
                        <p>Tu cuenta ha sido creada exitosamente. Para activarla, por favor confirma tu correo electrónico haciendo clic en el siguiente botón:</p>
                        <center>
                            <a href='{linkConfirmacion}' class='button'>Confirmar mi cuenta</a>
                        </center>
                        <p><small>O copia y pega este enlace en tu navegador:</small></p>
                        <p><small>{linkConfirmacion}</small></p>
                        <p><strong>⚠️ Este enlace expirará en 1 hora.</strong></p>
                        <p>Si no creaste esta cuenta, puedes ignorar este correo.</p>
                    </div>
                    <div class='footer'>
                        <p>Este es un correo automático, por favor no respondas.</p>
                        <p>&copy; 2025 SmartBook - Sistema de Gestión</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        // ========== ✨ NUEVO: ENVIAR CORREO ==========
        try
        {
            await _emailService.EnviarCorreo(
                usuario.CorreoUsuario,
                "Confirma tu cuenta - SmartBook",
                cuerpoCorreo
            );
        }
        catch (Exception ex)
        {
            // Buscar luego la manera de hacer una transaccion 
            throw new BadRequestException("Error al crear usuario. Intenta de nuevo.");
            // El usuario fue creado, pero el correo no se envió
        }

        // ========== ✨ NUEVO: RETORNAR RESPUESTA CON MENSAJE ==========
        return new UsuarioResponse(usuario.NombresUsuario, usuario.CorreoUsuario, "Usuario creado exitosamente. Por favor revisa tu correo para confirmar tu cuenta.");

    }
    





    public IEnumerable<ConsultarUsuarioResponse> Consultar(ConsultarUsuarioRequest request)
    {

 
        // Si llegó aquí, hay resultados
        return _usuarioRepository.Consultar(request); ;
    }


    public async Task<bool> Actualizar(string IdentificacionUsuario, ActualizarUsuarioRequest request)
    {
        // Obtener el usuario objetivo
        var usuarioObjetivo = await _usuarioRepository.ObtenerPorIdentificacion(IdentificacionUsuario);

        if (usuarioObjetivo == null)
            throw new NotFoundException("El usuario no existe.");

        // Obtener información del usuario autenticado
        var usuarioActual = _httpContextAccessor.HttpContext?.User;
        var rolActual = usuarioActual?.FindFirst(ClaimTypes.Role)?.Value;
        var estaAutenticado = usuarioActual?.Identity?.IsAuthenticated ?? false;

        // ✔ Validación: Debe estar autenticado
        if (!estaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión para actualizar usuarios.");

        // ✔ Validación: Solo Admins pueden actualizar usuarios
        if (rolActual != "Admin")
            throw new ForbiddenExeption("Solo un administrador puede actualizar usuarios.");

        // Actualizar en el repositorio
        return await _usuarioRepository.Actualizar(IdentificacionUsuario, request);  // ✅ await
    }





    public async Task<ConfirmarEmailResponse> ConfirmarEmail(ConfirmarEmailRequest request)
    {
        // ========== VALIDACIONES ==========

        // 1. Buscar usuario por token
        var usuario = await _usuarioRepository.ObtenerPorToken(request.Token);

        // 2. Validar que existe
        if (usuario == null)
        {
            throw new BadRequestException("Token inválido");
        }

        // 3. Validar que no haya expirado
        if (usuario.TokenExpiracion < DateTime.Now)
        {
            throw new BadRequestException("El token ha expirado");
        }

        // 4. Validar que no esté ya confirmado
        if (usuario.EmailConfirmado)
        {
            return new ConfirmarEmailResponse("Tu cuenta ya estaba confirmada");
        }

        // ========== ACTUALIZAR USUARIO ==========

        usuario.EmailConfirmado = true;
        usuario.TokenConfirmacion = null;
        usuario.TokenExpiracion = null;

        await _usuarioRepository.ActualizarEstadoCorreo(usuario);

        // ========== ENVIAR CORREO DE CONFIRMACIÓN ==========

        var cuerpoCorreo = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
.logo {{ text-align: center; padding: 20px 0; }}
.logo img {{ max-width: 180px; height: auto; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; background-color: #f9f9f9; }}
                .success-icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
                .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
<div class='logo'>
    <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRFs1EpEDt7-pfZ8rTh2qnwEUvvC45K8RhuZw&s' alt='CECAR Logo'>
</div>
                <div class='header'>
                    <h1>¡Cuenta Confirmada!</h1>
                </div>
                <div class='content'>
                    <div class='success-icon'>✅</div>
                    <h2>¡Felicidades {usuario.NombresUsuario}!</h2>
                    <p>Tu cuenta ha sido confirmada exitosamente.</p>
                    <p>Ya puedes iniciar sesión en SmartBook.</p>
                    <p><strong>Correo:</strong> {usuario.CorreoUsuario}</p>
                </div>
                <div class='footer'>
                    <p>&copy; 2025 SmartBook</p>
                </div>
            </div>
        </body>
        </html>
    ";

        try
        {
            await _emailService.EnviarCorreo(
                usuario.CorreoUsuario,
                "¡Cuenta confirmada! - SmartBook",
                cuerpoCorreo
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
        }

        // ========== RETORNAR RESPUESTA ==========

        return new ConfirmarEmailResponse(
            "¡Tu cuenta ha sido confirmada exitosamente! Ya puedes iniciar sesión."
        );
    }




    public async Task<LoginResponse> Login(LoginRequest request)
    {
        // ========== VALIDACIONES ==========

        // 1. Buscar usuario por correo
        var usuario = await _usuarioRepository.ObtenerPorCorreo(request.CorreoUsuario);

        if (usuario == null)
        {
            throw new NotFoundException("Correo o contraseña incorrectos");
        }

        // 2. Verificar que el email esté confirmado
        if (!usuario.EmailConfirmado)
        {
            throw new BadRequestException("Debes confirmar tu correo antes de iniciar sesión");
        }

        // 3. Verificar la contraseña
        bool contrasenaValida = PasswordHelper.Verificar(
            request.ContraseniaUsuario,
            usuario.ContraseniaUSuario
        );

        if (!contrasenaValida)
        {
            throw new BadRequestException("Correo o contraseña incorrectos");
        }

        // ========== GENERAR TOKEN JWT ==========

        var token = JwtHelper.GenerarToken(usuario, _jwtOption);

        // ========== RETORNAR RESPUESTA ==========

        return new LoginResponse(
            token,
            usuario.NombresUsuario,
            usuario.CorreoUsuario,
            usuario.RolUsuario.ToString()
        );
    }







    public async Task<UsuarioResponse> RestablecerContrasena(string IdentificacionUsuario, RestablecerContrasenaRequest request)
    {
        // 1. Buscar usuario
        var usuario = await _usuarioRepository.ObtenerPorIdentificacion(IdentificacionUsuario);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario no encontrado");
        }

        // 2. Verificar contraseña actual
        if (!PasswordHelper.Verificar(request.ContrasenaActual, usuario.ContraseniaUSuario))
        {
            throw new BadRequestException("La contraseña actual es incorrecta");
        }

        // 3. Validar nueva contraseña
        if (request.ContrasenaNueva.Length < 8)
        {
            throw new BadRequestException("La contraseña debe tener mínimo 8 caracteres");
        }

        // 4. Validar confirmación
        if (request.ContrasenaNueva != request.ConfirmarContrasena)
        {
            throw new BadRequestException("Las contraseñas no coinciden");
        }

        // 5. Encriptar nueva contraseña
        usuario.ContraseniaUSuario = PasswordHelper.Encriptar(request.ContrasenaNueva);

        // 6. Guardar
        await _usuarioRepository.ActualizarContraseña(usuario);

        // 7. Enviar correo de notificación
        var cuerpoCorreo = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
.logo {{ text-align: center; padding: 20px 0; }}
.logo img {{ max-width: 180px; height: auto; }}
                .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; background-color: #f9f9f9; }}
                .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
<div class='logo'>
    <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRFs1EpEDt7-pfZ8rTh2qnwEUvvC45K8RhuZw&s' alt='CECAR Logo'>
</div>
                <div class='header'>
                    <h1>Contraseña Restablecida</h1>
                </div>
                <div class='content'>
                    <h2>Hola {usuario.NombresUsuario},</h2>
                    <p>Te confirmamos que tu contraseña ha sido restablecida exitosamente.</p>
                    <p>Si no realizaste este cambio, por favor contacta inmediatamente con soporte.</p>
                    <p><strong>Fecha del cambio:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                <div class='footer'>
                    <p>Este es un correo automático, por favor no respondas.</p>
                    <p>&copy; 2025 SmartBook - Sistema de Gestión</p>
                </div>
            </div>
        </body>
        </html>
    ";

        try
        {
            await _emailService.EnviarCorreo(
                usuario.CorreoUsuario,
                "Contraseña restablecida - SmartBook",
                cuerpoCorreo
            );
        }
        catch (Exception)
        {
            // El cambio fue exitoso pero el correo falló
            // No bloqueamos la operación
        }

        return new UsuarioResponse(
            usuario.NombresUsuario,
            usuario.CorreoUsuario,
            "Contraseña restablecida correctamente"
        );
    }






}
