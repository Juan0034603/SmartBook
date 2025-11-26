using Microsoft.AspNetCore.Mvc;
using SmartBook.Application.Services;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Exceptions;

namespace SmartBook.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IngresosController : ControllerBase
{
    private readonly IngresoService _ingresoService;

    public IngresosController(IngresoService ingresoService)
    {
        _ingresoService = ingresoService;
    }

    [HttpPost]
    public ActionResult Crear(CrearIngresoRequest request)
    {
        try
        {
            var ingreso = _ingresoService.Crear(request);
            if (ingreso is null)
            {
                return BadRequest();
            }
            return Created(string.Empty, ingreso);
        }
        catch (BusinessRoleException exb)
        {
            return UnprocessableEntity(exb.Message);
        }
        catch (Exception exg)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Ocurrió un error al procesar la solicitud");
        }
    }

    [HttpGet("{id}")]
    public ActionResult Consultar(string id)
    {
        try
        {
            var ingreso = _ingresoService.Consultar(id);

            if (ingreso is null)
            {
                return NotFound(new { mensaje = $"No se encontró el ingreso con ID {id}" });
            }

            return Ok(ingreso);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Ocurrió un error al procesar la solicitud");
        }
    }

    [HttpGet]
    public ActionResult Consultar([FromQuery] ConsultarIngresoRequest request)
    {
        try
        {
            var ingresos = _ingresoService.Consultar(request);
            return Ok(ingresos);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Ocurrió un error al procesar la solicitud");
        }
    }
}