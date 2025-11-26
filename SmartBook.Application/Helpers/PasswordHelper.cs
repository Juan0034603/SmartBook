//buscar pq no se usar using

namespace SmartBook.Application.Helpers;

public static class PasswordHelper
{

    public static string Encriptar(string contrasena)
    {
        // Usa BCrypt para crear el hash
        // Retorna el hash seguro
        return BCrypt.Net.BCrypt.HashPassword(contrasena);
    }

    // Método 2: Verificar contraseña
    public static bool Verificar(string contrasena, string hash)
    {
        // Usa BCrypt para comparar
        // Retorna true si coinciden, false si no
        return BCrypt.Net.BCrypt.Verify(contrasena, hash);
    }


    }

