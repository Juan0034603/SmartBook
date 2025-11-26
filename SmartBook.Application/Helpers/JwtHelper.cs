using Microsoft.IdentityModel.Tokens;
using SmartBook.Application.Options;
using SmartBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Helpers;
public static class JwtHelper
{
    public static string GenerarToken(Usuario usuario, JwtOption jwtOption)
    {
        // 1. Crear los claims (información del usuario en el token)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario),
            new Claim(ClaimTypes.Email, usuario.CorreoUsuario),
            new Claim(ClaimTypes.Name, usuario.NombresUsuario),
            new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString())
        };

        // 2. Crear la clave de firma
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Crear el token
        var token = new JwtSecurityToken(
            issuer: jwtOption.Issuer,
            audience: jwtOption.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(jwtOption.ExpirationMinutes),
            signingCredentials: credentials
        );

        // 4. Convertir a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}