using Microsoft.AspNetCore.Mvc;
using SmartBook.Application.Services;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Exceptions;

namespace SmartBook.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LibrosController : ControllerBase
{
    private readonly LibroService _libroService;

    public LibrosController(LibroService libroService)
    {
        _libroService = libroService;
    }

    [HttpPost]
    public ActionResult Crear(CrearLibroRequest request)
    {
        try
        {
            var libro = _libroService.Crear(request);

            if (libro is null)
            {
                return BadRequest();
            }

            return Created(string.Empty, libro);
        }
        catch (BusinessRoleException exb)
        {
            return UnprocessableEntity(exb.Message);
        }
        catch (Exception exg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, exg.Message);
        }
    }

    [HttpGet("{id}")]
    public ActionResult Consultar(string id)
    {
        var libro = _libroService.Consultar(id);

        if (libro is null)
        {
            return NotFound();
        }

        return Ok(libro);
    }

    [HttpGet]
    public ActionResult Consultar([FromQuery] ConsultarLibroRequest request)
    {
        return Ok(_libroService.Consultar(request));
    }

    [HttpPut("{id}")]
    public ActionResult Actualizar(string id, ActualizarLibroRequest request)
    {
        try
        {
            var actualizado = _libroService.Actualizar(id, request);

            if (!actualizado)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (BusinessRoleException exb)  
        {
            return UnprocessableEntity(exb.Message);  // Retorna 422 con el mensaje
        }
        catch (Exception exg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
        }
    }


}