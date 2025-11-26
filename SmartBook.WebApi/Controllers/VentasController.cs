using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBook.Application.Services;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Exceptions;

namespace SmartBook.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]

public class VentasController : ControllerBase
{
    private readonly VentaService _ventaService;

    public VentasController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    // POST: api/ventas
    [HttpPost]
    public ActionResult Crear(CrearVentaRequest request)
    {
        try
        {
            var venta = _ventaService.Crear(request);
            if (venta is null)
            {
                return BadRequest();
            }
            return Created(string.Empty, venta);
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

    // GET: api/ventas/{id}
    [HttpGet("{id}")]
    public ActionResult Consultar(string id)
    {
        var venta = _ventaService.Consultar(id);
        if (venta is null)
        {
            return NotFound();
        }
        return Ok(venta);
    }

    // GET: api/ventas?desde=2025-10-1&hasta=2025-11-11&clienteId=abc123&libroId=libro1
    [HttpGet]
    public ActionResult Consultar([FromQuery] ConsultarVentaRequest request)
    {
        return Ok(_ventaService.Consultar(request));
    }
}
