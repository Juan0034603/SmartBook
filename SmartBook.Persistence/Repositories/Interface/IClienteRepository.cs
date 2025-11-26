using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using SmartBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Persistence.Repositories.Interface;

public interface IClienteRepository
{
    void Crear(Cliente cliente);
    bool ValidarCreacionCliente(string identificacion, string email, string celular);
    ConsultarClienteResponse? Consultar(string identificacion);
    IEnumerable<ConsultarClienteResponse> Consultar(ConsultarClienteRequest request);
    bool Actualizar(string identificacion, ActualizarClienteRequest request);

}