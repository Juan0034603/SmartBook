using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBook.Application.Interface;
using SmartBook.Application.Services;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Exceptions;
using System.Security.Claims;


namespace SmartBook.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase

{
    //Inyeccion de dependencias, (como una funcion pero mas potente), (No copia ni ejecuta codigo, Recive instancia lista para usar )
    // es como una caja de herramientas: el controlador no copia su código ni lo ejecuta solo, sino que recibe una instancia lista para usar gracias a la inyección de dependencias. El controlador llama a los métodos del servicio cuando los necesita (como usar herramientas de la caja), obtiene los resultados y los devuelve en la respuesta HTTP. Después, esa instancia queda disponible para la siguiente petición; así puedes separar responsabilidades y reutilizar el servicio sin crear objetos manualmente.
    private readonly UsuarioService _usuarioService;

    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;

    }


    [HttpPost]
    [Authorize]  // ← Requiere token JWT válido

    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
    {
        try
        {
            var usuario = await _usuarioService.Crear(request);

            if (usuario is null)
            {
                return NotFound();
            }
            return Created(string.Empty, usuario);
        }
        catch (BadRequestException exb)
        {
            return UnprocessableEntity(exb.Message);
        }
        catch (Exception exg)
        {

            return StatusCode(StatusCodes.Status500InternalServerError, exg.Message);
        }
    }


    //Busacar pq es esata la estruvtura
    [HttpGet]
    [Authorize]  // ← Requiere token JWT válido

    public ActionResult Consultar([FromQuery] ConsultarUsuarioRequest request)
    {
        //al ser consulta flexible no usamos 404 not found

        return Ok(_usuarioService.Consultar(request));
    }


    [HttpPut("{IdentificacionUsuario}")]
    [Authorize]
    public async Task<ActionResult> Actualizar(string IdentificacionUsuario, ActualizarUsuarioRequest request)
    {
        var resultado = await _usuarioService.Actualizar(IdentificacionUsuario, request);  // ✅ await
        if (!resultado)
        {
            return NotFound();
        }
        return NoContent();
    }




    [HttpPut("confirmar-email")]
    public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailRequest request)
    {
        try
        {
            var resultado = await _usuarioService.ConfirmarEmail(request);

            if (resultado is null)
            {
                return NotFound();
            }
            return Ok(resultado);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }

    }




    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var resultado = await _usuarioService.Login(request);
            return Ok(resultado);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al iniciar sesión" });
        }
    }


    [HttpPut("restablecer-contrasena")]
    [Authorize]  // Usuario debe estar logueado
    public async Task<IActionResult> RestablecerContrasena([FromBody] RestablecerContrasenaRequest request)
    {
        try
        {
            // Obtener ID del usuario del token
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized();
            }

            var resultado = await _usuarioService.RestablecerContrasena(usuarioId, request);
            return Ok(resultado);
        }
        catch (BadRequestException exb)
        {
            return UnprocessableEntity(exb.Message);
        }
        catch (Exception exg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, exg.Message);
        }
    }




}
