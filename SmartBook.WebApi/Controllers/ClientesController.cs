using Microsoft.AspNetCore.Mvc;
using SmartBook.Application.Services;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Exceptions;

namespace SmartBook.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClientesController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public ActionResult Crear(CreateClienteRequest request)
    {
        try
        {
            var cliente = _clienteService.Crear(request);
            if (cliente is null)
            {
                return BadRequest();
            }
            return Created(string.Empty, cliente);
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


    [HttpGet("{identificacion}")]
    public ActionResult Consultar(string identificacion)
    {
        var cliente = _clienteService.Consultar(identificacion);

        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }

    [HttpGet]
    public ActionResult Consultar([FromQuery] ConsultarClienteRequest request)
    {
        var clientes = _clienteService.Consultar(request);
        return Ok(clientes);
    }

    [HttpPut("{identificacion}")]
    public ActionResult Actualizar(string identificacion, ActualizarClienteRequest request)
    {
        
            var cliente = _clienteService.Actualizar(identificacion, request);

            if (!cliente)
            {
                return NotFound();
            }

            return NoContent();
        }
        
    }

