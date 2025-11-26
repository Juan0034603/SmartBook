using System.Text.RegularExpressions;
using SmartBook.Application.Extensions;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Exceptions;
using SmartBook.Persistence.Repositories.Interface;

namespace SmartBook.Application.Services;

public class ClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public ClienteResponse? Crear(CreateClienteRequest request)
    {
        // Validación 1: Email válido
        if (!ValidarEmail(request.Email))
        {
            throw new BusinessRoleException("El formato del email no es válido");
        }

        // Validación 2: Edad mínima
        var edad = CalcularEdad(request.FechaNacimiento);
        if (edad < 14)
        {
            throw new BusinessRoleException("No se puede registrar clientes menores de 14 años");
        }

        // Validación 3: Celular (10 dígitos)
        if (request.Celular.Length != 10 || !request.Celular.All(char.IsDigit))
        {
            throw new BusinessRoleException("El celular debe tener exactamente 10 dígitos");
        }

        // Validación 4: Unicidad
        var clienteValido = _clienteRepository.ValidarCreacionCliente(
            request.Identificacion,
            request.Email,
            request.Celular
        );

        if (!clienteValido)
        {
            throw new BusinessRoleException("Ya existe un cliente con la identificación, email o celular ingresado");
        }

        var cliente = new Cliente
        {
            Id = DateTime.Now.Ticks.ToString(),
            Identificacion = request.Identificacion.Sanitize(),
            Nombres = request.Nombres.Sanitize().RemoveAccents(),
            Email = request.Email.Sanitize(),
            Celular = request.Celular.Sanitize(),
            FechaNacimiento = request.FechaNacimiento,
            FechaCreacion = DateTime.Now
        };

        _clienteRepository.Crear(cliente);

        return new ClienteResponse(cliente.Id, cliente.Nombres, cliente.Email);
    }

    private int CalcularEdad(DateOnly fechaNacimiento)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
        return edad;
    }

    private bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }


    public ConsultarClienteResponse? Consultar(string identificacion)
    {
        return _clienteRepository.Consultar(identificacion);
    }

    public IEnumerable<ConsultarClienteResponse> Consultar(ConsultarClienteRequest request)
    {
        return _clienteRepository.Consultar(request);
    }

    public bool Actualizar(string identificacion, ActualizarClienteRequest request)
    {
        return _clienteRepository.Actualizar(identificacion, request);
    }


}