using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Interface;

public interface IEmailService
{

    Task<bool> EnviarCorreo(string destinatario, string asunto, string cuerpo);



}
