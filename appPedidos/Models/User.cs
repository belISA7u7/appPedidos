namespace appPedidos.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; }
}