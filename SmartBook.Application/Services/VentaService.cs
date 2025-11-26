using SmartBook.Application.Extensions;
using SmartBook.Application.Interface;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using SmartBook.Domain.Exceptions;
using SmartBook.Persistence.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Services;

public class VentaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IIngresoRepository _ingresoRepository;
    private readonly ILibroRepository _libroRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IEmailService _emailService;

    public VentaService(
        IVentaRepository ventaRepository,
        IIngresoRepository ingresoRepository,
        ILibroRepository libroRepository,
        IClienteRepository clienteRepository,
        IEmailService emailService)
    {
        _ventaRepository = ventaRepository;
        _ingresoRepository = ingresoRepository;
        _libroRepository = libroRepository;
        _clienteRepository = clienteRepository;
        _emailService = emailService;
    }

    public VentaResponse? Crear(CrearVentaRequest request)
    {
        // Validación 1: Verificar que el cliente existe
        var cliente = _clienteRepository.Consultar(request.ClienteIdentificacion);
        if (cliente is null)
        {
            throw new BusinessRoleException("El cliente especificado no existe");
        }

        // Validación 2: Verificar stock disponible para cada libro
        foreach (var detalle in request.Detalles)
        {
            var stockDisponible = _ingresoRepository.ObtenerStockDisponible(detalle.LibroId);
            if (stockDisponible < detalle.Cantidad)
            {
                var libro = _libroRepository.Consultar(detalle.LibroId);
                var nombreLibro = libro?.NombreLibro ?? "desconocido";
                throw new BusinessRoleException(
                    $"Stock insuficiente para el libro '{nombreLibro}'. Disponible: {stockDisponible}, Solicitado: {detalle.Cantidad}");
            }
        }

        // Crear la venta (encabezado)
        var venta = new VentaLibro
        {
            Id = Guid.NewGuid().ToString(),
            NumeroReciboPago = request.NumeroReciboPago.Sanitize(),
            Fecha = DateTime.Now,
            ClienteIdentificacion = request.ClienteIdentificacion,
            UsuarioId = request.UsuarioId,
            Observaciones = request.Observaciones?.Sanitize(),
            FechaCreacion = DateTime.Now,
            Detalles = new List<DetalleVenta>()
        };

        // Procesar cada libro solicitado (aplicar FIFO)
        foreach (var detalleRequest in request.Detalles)
        {
            var libro = _libroRepository.Consultar(detalleRequest.LibroId);
            if (libro is null)
            {
                throw new BusinessRoleException($"El libro con ID {detalleRequest.LibroId} no existe");
            }

            // Obtener lotes disponibles ordenados por FIFO (más viejo primero)
            var lotes = _ingresoRepository.ObtenerLotesPorLibro(detalleRequest.LibroId);

            var cantidadRestante = detalleRequest.Cantidad;
            var totalDescontado = 0;

            // Descontar de cada lote según FIFO
            foreach (var lote in lotes)
            {
                if (cantidadRestante <= 0) break;

                var cantidadADescontar = Math.Min(cantidadRestante, lote.Unidades);

                // Crear detalle de venta para este lote
                var detalleVenta = new DetalleVenta
                {
                    Id = Guid.NewGuid().ToString(),
                    LibroId = detalleRequest.LibroId,
                    Lote = lote.Ingreso.Lote,
                    Cantidad = cantidadADescontar,
                    PrecioUnitario = lote.ValorVentaPublico,
                    FechaCreacion = DateTime.Now
                };

                venta.Detalles.Add(detalleVenta);

                // Descontar del inventario (DetalleIngresos)
                _ingresoRepository.DescontarUnidades(lote.IdDetalleIngreso, cantidadADescontar);

                cantidadRestante -= cantidadADescontar;
                totalDescontado += cantidadADescontar;
            }

            // Actualizar stock general del libro
            _libroRepository.ActualizarStockVenta(libro.IdLibro, libro.StockLibro - totalDescontado);
        }

        // Guardar la venta completa (encabezado + detalles)
        _ventaRepository.Crear(venta);

        // Generar y enviar PDF por email
        EnviarNotificacionVenta(venta, cliente);

        // Retornar response
        return MapearVentaResponse(venta, cliente);
    }

    public VentaResponse? Consultar(string id)
    {
        return _ventaRepository.Consultar(id);
    }

    public IEnumerable<VentaResponse> Consultar(ConsultarVentaRequest request)
    {
        return _ventaRepository.Consultar(request);
    }

    private VentaResponse MapearVentaResponse(VentaLibro venta, ConsultarClienteResponse cliente)
    {
        var detallesResponse = venta.Detalles.Select(d =>
        {
            var libro = _libroRepository.Consultar(d.LibroId);
            return new VentaDetalleResponse(
                libro?.NombreLibro ?? "Desconocido",
                libro?.NivelLibro ?? "",
                d.Lote,
                d.Cantidad,
                d.PrecioUnitario,
                d.Cantidad * d.PrecioUnitario
            );
        }).ToList();

        var montoTotal = detallesResponse.Sum(d => d.Subtotal);

        return new VentaResponse(
            venta.NumeroReciboPago,
            venta.Fecha,
            cliente.Nombres,
            cliente.Email,
            venta.Observaciones,
            detallesResponse,
            montoTotal
        );
    }

    private async void EnviarNotificacionVenta(VentaLibro venta, ConsultarClienteResponse cliente)
    {
        try
        {
            var cuerpoEmail = GenerarCuerpoEmailHTML(venta, cliente);

            await _emailService.EnviarCorreo(
                destinatario: cliente.Email,
                asunto: $"Confirmación de Compra - Recibo #{venta.NumeroReciboPago}",
                cuerpo: cuerpoEmail
            );
        }
        catch (Exception ex)
        {
            // Log del error pero no detener el proceso de venta
            Console.WriteLine($"Error al enviar email: {ex.Message}");
        }
    }

    private string GenerarCuerpoEmailHTML(VentaLibro venta, ConsultarClienteResponse cliente)
    {
        var detalles = venta.Detalles.Select(d =>
        {
            var libro = _libroRepository.Consultar(d.LibroId);
            return $@"
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{libro?.NombreLibro}</td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{d.Lote}</td>
                    <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{d.Cantidad}</td>
                    <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>${d.PrecioUnitario:N0}</td>
                    <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>${d.Cantidad * d.PrecioUnitario:N0}</td>
                </tr>";
        });

        var total = venta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Confirmación de Compra</h2>
                    <p>Estimado/a <strong>{cliente.Nombres}</strong>,</p>
                    <p>Gracias por su compra. A continuación el detalle de su transacción:</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; margin: 20px 0;'>
                        <p><strong>Número de Recibo:</strong> {venta.NumeroReciboPago}</p>
                        <p><strong>Fecha:</strong> {venta.Fecha:dd/MM/yyyy HH:mm}</p>
                    </div>

                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                        <thead>
                            <tr style='background-color: #4CAF50; color: white;'>
                                <th style='padding: 10px; border: 1px solid #ddd;'>Libro</th>
                                <th style='padding: 10px; border: 1px solid #ddd;'>Lote</th>
                                <th style='padding: 10px; border: 1px solid #ddd;'>Cantidad</th>
                                <th style='padding: 10px; border: 1px solid #ddd;'>Precio Unit.</th>
                                <th style='padding: 10px; border: 1px solid #ddd;'>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", detalles)}
                        </tbody>
                        <tfoot>
                            <tr style='background-color: #f9f9f9; font-weight: bold;'>
                                <td colspan='4' style='padding: 10px; border: 1px solid #ddd; text-align: right;'>TOTAL:</td>
                                <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>${total:N0}</td>
                            </tr>
                        </tfoot>
                    </table>

                    <p style='margin-top: 30px; color: #666; font-size: 12px;'>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </body>
            </html>";
    }
}