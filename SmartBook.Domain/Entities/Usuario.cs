using SmartBook.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartBook.Domain.Entities;

public class Usuario
{
    
    [Key]
    [Required]
    public string IdUsuario { get; init; }
    [Required]
    public string IdentificacionUsuario { get; init; }

    [Required]
    public string NombresUsuario { get; set; }

    [Required]
    [MinLength(8)]
    public string ContraseniaUSuario { get; set; }

    [Required]
    // [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]   
    [EmailAddress]
    public string CorreoUsuario { get; set; }
    [Required]
    public RolUsuario RolUsuario { get; set; }

    [Required]
    public EstadoUsuario EstadoUsuario { get; set; }  // Por ahora siempre activa

    [Required]
    public DateTime FechaCreacion { get; init; }

   public bool  EmailConfirmado { get; set; }
    public string? TokenConfirmacion { get; set; }
	public DateTime? TokenExpiracion { get; set; }
}
