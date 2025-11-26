using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Options;

public class SmtpSettings
{
    //= string.Empty; Inicializa la propiedad con uin string vacio Problema: Si no hay valor en appsettings.json, será null → puede causar errores.
//    Con inicialización:
//No es obligatorio, mas si es buena practica
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
    public string CorreoRemitente { get; set; } = string.Empty;

}
