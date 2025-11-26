using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartBook.Application.Helpers;

public class CorreoHelper
{
    // Validar formato de correo
    public static bool EsFormatoValido(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return false;

        return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    // Validar si es correo institucional
    public static bool EsCorreoInstitucional(string correo, List<string> dominiosPermitidos)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return false;

        correo = correo.ToLowerInvariant();

        foreach (var dominio in dominiosPermitidos)
        {
            if (correo.EndsWith(dominio.ToLowerInvariant()))
                return true;
        }

        return false;
    }

}
